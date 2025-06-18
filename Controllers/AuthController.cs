using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration; // JWT
    private readonly IDistributedCache _distributedCache; //Redis

    public AuthController(AppDbContext context, IUserService userService, ILogger<AuthController> logger, IConfiguration configuration, IDistributedCache distributedCache)
    {
        _context = context;
        _userService = userService;
        _logger = logger;
        _configuration = configuration; //JWT
        _distributedCache = distributedCache; //Redis
    }

    [HttpPost("login")] // api/auth/login 
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
    {

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userToLogin = await _context.Users.FirstOrDefaultAsync(u => u.Username == userLoginDto.Username);

        if (userToLogin == null)
        {
            _logger.LogWarning($"Login attempt failed for non-existent username: {userLoginDto.Username}");
            return Unauthorized(new { message = "Invalid username or password." });
        }

        if (!userToLogin.IsActive)
        {
            _logger.LogWarning($"Login attempt for inactive user: {userLoginDto.Username}");
            return Unauthorized(new { message = "User account is inactive." });
        }

        if (!BCrypt.Net.BCrypt.Verify(userLoginDto.Password, userToLogin.PasswordHash))
        {
            _logger.LogWarning($"Login attempt failed. Invalid password for username: {userLoginDto.Username}");
            return Unauthorized(new { message = "Invalid username or password." });
        }

        _logger.LogInformation($"User {userLoginDto.Username} logged in successfully.");

        string createdToken = GenerateJwtToken(userToLogin);
        return Ok(new { token = createdToken });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var createdUserDto = await _userService.CreateUserAsync(userRegisterDto);

            _logger.LogInformation($"User {createdUserDto.Username} registered in successfully.");

            string createdToken = GenerateJwtTokenForDto(createdUserDto);
            return Ok(new { token = createdToken });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, $"Registration attempt failed: {ex.Message}");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during user registration.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred while processing your request." });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);

        if (string.IsNullOrEmpty(jti))
        {
            _logger.LogWarning("Logout attempt with a token missing JTI claim.");
            return BadRequest(new { message = "Token ID (JTI) not found in token." });
        }

        var expClaimValue = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

        if (string.IsNullOrEmpty(expClaimValue) || !long.TryParse(expClaimValue, out long expUnixTimestamp))
        {
            _logger.LogWarning($"Logout attempt for token JTI: {jti} with missing or invalid EXP claim.");
            return BadRequest(new { message = "Invalid token expiration claim." });
        }

        if (expUnixTimestamp <= 0) //0 || <0
        {
            _logger.LogWarning($"Logout attempt for token JTI: {jti} with non-positive EXP claim value: {expUnixTimestamp}.");
            return BadRequest(new { message = "Token expiration claim value must be positive." });
        }

        var expiryDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(expUnixTimestamp).UtcDateTime;
        var nowUtc = DateTime.UtcNow;

        TimeSpan remainingTime = expiryDateTimeUtc - nowUtc;

        if (remainingTime.Ticks > 0)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = remainingTime
            };
            await _distributedCache.SetStringAsync($"blacklist_{jti}", "canceled", options);
            _logger.LogInformation($"Token with JTI {jti} blacklisted. Expires in {remainingTime.TotalSeconds:F0} seconds.");
        }
        else
        {
            _logger.LogInformation($"Token with JTI {jti} has already expired. No need to blacklist.");
        }
        return Ok(new { message = "Successfully logged out." });
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var jwtKey = _configuration["Jwt:Key"];// read jwt/key from appsettings.json
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
        {
            _logger.LogError("JWT configuration (Key, Issuer, or Audience) is missing or empty.");
            throw new InvalidOperationException("JWT configuration is not properly set.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = credentials
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenObject = tokenHandler.CreateToken(tokenDescriptor);
        string tokenString = tokenHandler.WriteToken(tokenObject);
        _logger.LogInformation($"Token generated successfully for user: {user.Username}");
        return tokenString;
    }

    private string GenerateJwtTokenForDto(UserDto userDto)
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
        new Claim(ClaimTypes.Name, userDto.Username),
        new Claim(ClaimTypes.Email, userDto.Email),
        new Claim(ClaimTypes.Role, userDto.Role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var jwtKey = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
        {
            _logger.LogError("JWT configuration (Key, Issuer, or Audience) is missing or empty in appsettings.json.");
            throw new InvalidOperationException("JWT configuration is not properly set up.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenObject = tokenHandler.CreateToken(tokenDescriptor);
        string tokenString = tokenHandler.WriteToken(tokenObject);
        _logger.LogInformation($"Token generated successfully for newly registered user: {userDto.Username}");
        return tokenString;
    }
}