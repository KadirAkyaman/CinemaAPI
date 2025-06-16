using Microsoft.EntityFrameworkCore;
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
    public async Task<DirectorDto> CreateDirectorAsync(DirectorCreateDto directorCreateDto)
    {
        if (directorCreateDto == null)
            throw new ArgumentNullException(nameof(directorCreateDto));

        var newDirector = new Director
        {
            Name = directorCreateDto.Name,
            Surname = directorCreateDto.Surname
        };

        await _directorRepository.InsertAsync(newDirector);

        await _context.SaveChangesAsync();
        return new DirectorDto
        {
            Id = newDirector.Id,
            Name = newDirector.Name,
            Surname = newDirector.Surname
        };
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

    public async Task<IEnumerable<DirectorDto>> GetAllDirectorsAsync()
    {
        var directors = await _directorRepository.GetAllAsync();
        var directorDtos = directors.Select(director => new DirectorDto
        {
            Id = director.Id,
            Name = director.Name,
            Surname = director.Surname
        }).ToList();
        return directorDtos;
    }

    public async Task<DirectorDto?> GetDirectorByIdAsync(int id)
    {
        var director = await _directorRepository.GetByIdAsync(id);
        if (director == null)
            return null;

        var directorDto = new DirectorDto
        {
            Id = director.Id,
            Name = director.Name,
            Surname = director.Surname
        };
        return directorDto;
    }

    public async Task UpdateDirectorAsync(int id, DirectorUpdateDto directorUpdateDto)
    {
        if (directorUpdateDto == null)
            throw new ArgumentNullException(nameof(directorUpdateDto));

        var directorToUpdate = await _directorRepository.GetByIdAsync(id);
        if (directorToUpdate == null)
        {
            _logger.LogWarning($"Director with id {id} not found for update.");
            throw new KeyNotFoundException($"Director with id {id} not found.");
        }

        if (!string.IsNullOrWhiteSpace(directorUpdateDto.Name))
        {
            directorToUpdate.Name = directorUpdateDto.Name;
        }
        if (!string.IsNullOrWhiteSpace(directorUpdateDto.Surname))
        {
            directorToUpdate.Surname = directorUpdateDto.Surname;
        }

        await _directorRepository.Update(directorToUpdate);
        await _context.SaveChangesAsync();
    }


}