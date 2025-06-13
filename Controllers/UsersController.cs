using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsersAsync()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to retrieve all users.");
            return StatusCode(500, "An unexpected error occurred while processing your request to get all users.");
        }
    }

    [HttpGet("{id:int}", Name = "GetUserById")]
    public async Task<ActionResult<UserDto>> GetUserByIdAsync(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid user ID.");
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while trying to retrieve user with id {id}.");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUserAsync([FromBody] User user)
    {
        if (user == null)
            return BadRequest("User data cannot be null!");

        if (user.Id != 0)
            return BadRequest("User ID should not be set when creating a new movie.");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdUserDto = await _userService.CreateUserAsync(user);
            return CreatedAtRoute("GetUserById", new { id = createdUserDto.Id }, createdUserDto);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, $"Validation or business rule error while creating user: {user?.Username}");
            return BadRequest(argEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An unexpected error occurred while creating user. Attempted title: {user?.Username}");
            return StatusCode(500, "An internal error occurred while creating the user.");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateUserAsync(int id, [FromBody] User user)
    {
        if (id <= 0) // Bu kontrol eklenebilir
            return BadRequest("Invalid user ID.");

        if (user == null)
            return BadRequest("User data cannot be null!");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != user.Id)
            return BadRequest("The ID in the URL does not match the ID in the request body.");

        try
        {
            await _userService.UpdateUserAsync(id, user);
            return NoContent();
        }
        catch (KeyNotFoundException knfex)
        {
            _logger.LogWarning(knfex, $"User with id {id} not found for update.");
            return NotFound(knfex.Message);
        }
        catch (ArgumentException argex)
        {
            _logger.LogWarning(argex, "Invalid data for updating user.");
            return BadRequest(argex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating user with id {id}");
            return StatusCode(500, "An internal error occurred while updating the user.");
        }
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUserAsync(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid user ID.");
        try
        {
            var wasDeleted = await _userService.DeleteUserAsync(id);
            if (wasDeleted)
                return NoContent();
            else
                return NotFound($"User with id {id} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting user with id {id}");
            return StatusCode(500, "An internal error occurred while deleting the user.");
        }
    }
}