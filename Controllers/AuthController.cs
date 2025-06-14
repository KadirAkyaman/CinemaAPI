using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public AuthController(AppDbContext context, IUserService userService, ILogger<AuthController> logger, IConfiguration configuration)
    {
        _context = context;
        _userService = userService;
        _logger = logger;
        _configuration = configuration;
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
}