public interface IDirectorService
{
    Task<IEnumerable<Director>> GetAllDirectorsAsync();
    Task<Director?> GetDirectorByIdAsync(int id);
    Task<Director> CreateDirectorAsync(Director director);
    Task UpdateDirectorAsync(int id, Director director);
    Task<bool> DeleteDirectorAsync(int id);
}