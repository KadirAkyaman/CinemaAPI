
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
    public async Task<UserDto> CreateUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        await _userRepository.InsertAsync(user);

        await _context.SaveChangesAsync();

        var createdUserDto = new UserDto
        {
            Id = user.Id, // Artık ID'si var
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

    public async Task UpdateUserAsync(int id, User user)
    {
        var userToUpdate = await _userRepository.GetByIdAsync(id);
        if (userToUpdate == null)
        {
            _logger.LogWarning($"User with id {id} not found for update.");
            throw new KeyNotFoundException($"User with id {id} not found.");
        }
        userToUpdate.Username = user.Username;
        userToUpdate.Email = user.Email;
        userToUpdate.PasswordHash = user.PasswordHash;
        userToUpdate.Role = user.Role;
        userToUpdate.IsActive = user.IsActive;

        await _userRepository.UpdateAsync(userToUpdate);
        await _context.SaveChangesAsync();
    }
}