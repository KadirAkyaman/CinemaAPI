public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(User user);
    Task UpdateUserAsync(int id, User user);
    Task<bool> DeleteUserAsync(int id);
}