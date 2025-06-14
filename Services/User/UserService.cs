using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;
    public UserService(IRepository<User> userRepository, AppDbContext context, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _context = context;
        _logger = logger;
    }
    public async Task<UserDto> CreateUserAsync(UserRegisterDto userRegisterDto) // UserRegisterDto and Password hash with BCrypt
    {
        if (userRegisterDto == null)
            throw new ArgumentNullException(nameof(userRegisterDto));

        var user = new User
        {
            Username = userRegisterDto.Username,
            Email = userRegisterDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegisterDto.Password),
            Role = userRegisterDto.Role,
            IsActive = true
        };

        await _userRepository.InsertAsync(user);
        await _context.SaveChangesAsync();

        var createdUserDto = new UserDto
        {
            Id = user.Id, // 
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        };

        return createdUserDto;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var userToDelete = await _userRepository.GetByIdAsync(id);
        if (userToDelete == null)
        {
            _logger.LogWarning($"User with id {id} not found for deletion.");
            return false;
        }

        try
        {
            await _userRepository.DeleteAsync(id);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"User with id {id} deleted successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting entity with id {id}.");
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        if (users == null || !users.Any())
            return Enumerable.Empty<UserDto>();

        var userDtos = users.Select(user => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive
        }).ToList();

        return userDtos;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            return null;

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            Username = user.Username,
            IsActive = user.IsActive
        };

        return userDto;
    }

    public async Task UpdateUserAsync(int id, UserUpdateDto userUpdateDto)
    {
        var userToUpdate = await _userRepository.GetByIdAsync(id);
        if (userToUpdate == null)
        {
            _logger.LogWarning($"User with id {id} not found for update.");
            throw new KeyNotFoundException($"User with id {id} not found.");
        }
        if (userUpdateDto.Email != null)
            userToUpdate.Email = userUpdateDto.Email;

        if (userUpdateDto.Role != null)
        {
            userToUpdate.Role = userUpdateDto.Role;
        }
        if (userUpdateDto.IsActive.HasValue)
        {
            userToUpdate.IsActive = userUpdateDto.IsActive.Value;
        }
        if (!string.IsNullOrWhiteSpace(userUpdateDto.Password))
        {
            userToUpdate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userUpdateDto.Password);
            _logger.LogInformation($"Password updated for user with id {id}.");
        }
        if (!string.IsNullOrWhiteSpace(userUpdateDto.Username) && userUpdateDto.Username != userToUpdate.Username)
        {
            bool usernameExists = await _context.Users.AnyAsync(u => u.Id != id && u.Username == userUpdateDto.Username);
            if (usernameExists)
            {
                _logger.LogWarning($"Update failed for user {id}: Username '{userUpdateDto.Username}' is already in use.");
                throw new ArgumentException("Username is already in use by another account.");
            }
            userToUpdate.Username = userUpdateDto.Username;
        }

        await _userRepository.Update(userToUpdate);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"User with id {id} updated successfully.");
    }
}