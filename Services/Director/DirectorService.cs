
public class DirectorService : IDirectorService
{
    private readonly IRepository<Director> _directorRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<DirectorService> _logger;
    public DirectorService(IRepository<Director> directorRepository, AppDbContext context, ILogger<DirectorService> logger)
    {
        _directorRepository = directorRepository;
        _context = context;
        _logger = logger;
    }
    public async Task<Director> CreateDirectorAsync(Director director)
    {
        if (director == null)
            throw new ArgumentNullException(nameof(director));

        await _directorRepository.InsertAsync(director);

        await _context.SaveChangesAsync();
        return director;
    }

    public async Task<bool> DeleteDirectorAsync(int id)
    {
        var directorToDelete = await _directorRepository.GetByIdAsync(id);
        if (directorToDelete == null)
        {
            _logger.LogWarning($"Director with id {id} not found for deletion.");
            return false;
        }

        try
        {
            await _directorRepository.DeleteAsync(id);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Director with id {id} deleted successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting entity with id {id}.");
            throw;
        }
    }

    public async Task<IEnumerable<Director>> GetAllDirectorsAsync()
    {
        var directors = await _directorRepository.GetAllAsync();
        return directors;
    }

    public async Task<Director?> GetDirectorByIdAsync(int id)
    {
        var director = await _directorRepository.GetByIdAsync(id);
        return director;
    }

    public async Task UpdateDirectorAsync(int id, Director director)
    {
        var directorToUpdate = await _directorRepository.GetByIdAsync(id);
        if (directorToUpdate == null)
        {
            _logger.LogWarning($"Director with id {id} not found for update.");
            throw new KeyNotFoundException($"Director with id {id} not found.");
        }
        directorToUpdate.Name = director.Name;
        directorToUpdate.Surname = director.Surname;

        await _directorRepository.Update(directorToUpdate);
        await _context.SaveChangesAsync();
    }


}