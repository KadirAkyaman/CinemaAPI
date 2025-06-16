using Microsoft.EntityFrameworkCore;
public class MovieService : IMovieService
{
    private readonly IRepository<Movie> _movieRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<MovieService> _logger;
    public MovieService(IRepository<Movie> movieRepository, AppDbContext context, ILogger<MovieService> logger)
    {
        _movieRepository = movieRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<MovieDto> CreateMovieAsync(MovieCreateDto movieCreateDto)
    {
        if (movieCreateDto == null)
        {
            _logger.LogError("movieCreateDto cannot be null in CreateMovieAsync.");
            throw new ArgumentNullException(nameof(movieCreateDto));
        }
        if (movieCreateDto.DirectorId <= 0) // DirectorId'nin pozitif olması beklenir
        {
            _logger.LogWarning($"Invalid DirectorId {movieCreateDto.DirectorId} provided for new movie.");
            throw new ArgumentException("DirectorId must be a positive integer.", nameof(movieCreateDto.DirectorId));
        }
        var directorExists = await _context.Directors.AnyAsync(d => d.Id == movieCreateDto.DirectorId);
        if (!directorExists)
        {
            _logger.LogWarning($"Director with ID {movieCreateDto.DirectorId} not found. Cannot create movie.");
            throw new ArgumentException($"Director with ID {movieCreateDto.DirectorId} not found.", nameof(movieCreateDto.DirectorId));
        }
        var newMovie = new Movie
        {
            Title = movieCreateDto.Title,
            Description = movieCreateDto.Description,
            ReleaseDate = movieCreateDto.ReleaseDate,
            Genre = movieCreateDto.Genre,
            DirectorId = movieCreateDto.DirectorId
        };
        if (newMovie.ReleaseDate.Kind == DateTimeKind.Unspecified)
        {
            newMovie.ReleaseDate = DateTime.SpecifyKind(newMovie.ReleaseDate, DateTimeKind.Utc);
        }
        else if (newMovie.ReleaseDate.Kind == DateTimeKind.Local)
        {
            newMovie.ReleaseDate = newMovie.ReleaseDate.ToUniversalTime();
        }

        await _movieRepository.InsertAsync(newMovie);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Movie with ID {newMovie.Id} and Title '{newMovie.Title}' created successfully.");

        return new MovieDto
        {
            Id = newMovie.Id,
            Title = newMovie.Title,
            Description = newMovie.Description,
            ReleaseDate = newMovie.ReleaseDate,
            Genre = newMovie.Genre,
            DirectorId = newMovie.DirectorId
        };
    }

    public async Task<bool> DeleteMovieAsync(int id)
    {
        var movieToDelete = await _movieRepository.GetByIdAsync(id);

        if (movieToDelete == null)
        {
            _logger.LogWarning($"Movie with ID {id} not found for deletion.");
            return false;
        }

        try
        {
            await _movieRepository.DeleteAsync(id);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Movie with ID {id} deleted successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting movie with ID {id}.");
            throw;
        }
    }

    public async Task<IEnumerable<MovieDto>> GetAllMoviesAsync()
    {
        var movies = await _movieRepository.GetAllAsync();
        var movieDtos = movies.Select(movie => new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Genre = movie.Genre,
            DirectorId = movie.DirectorId
        }).ToList();
        return movieDtos;
    }

    public async Task<MovieDto?> GetMovieByIdAsync(int id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie == null)
            return null;
        var movieDto = new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Genre = movie.Genre,
            DirectorId = movie.DirectorId
        };

        return movieDto;
    }

    public async Task UpdateMovieAsync(int id, MovieUpdateDto movieUpdateDto)
    {
        if (movieUpdateDto == null)
        {
            _logger.LogError($"movieUpdateDto cannot be null for updating movie with ID {id}.");
            throw new ArgumentNullException(nameof(movieUpdateDto));
        }

        var movieToUpdate = await _movieRepository.GetByIdAsync(id);

        if (movieToUpdate == null)
        {
            _logger.LogWarning($"Movie with ID {id} not found for update.");
            throw new KeyNotFoundException($"Movie with ID {id} not found.");
        }

        if (!string.IsNullOrWhiteSpace(movieUpdateDto.Title))
        {
            movieToUpdate.Title = movieUpdateDto.Title;
        }

        movieToUpdate.Description = movieUpdateDto.Description;

        if (!string.IsNullOrWhiteSpace(movieUpdateDto.Genre))
        {
            movieToUpdate.Genre = movieUpdateDto.Genre;
        }
        else if (movieUpdateDto.Genre == string.Empty)
        {
            movieToUpdate.Genre = null;
        }

        if (movieUpdateDto.ReleaseDate.HasValue)
        {
            DateTime newReleaseDate = movieUpdateDto.ReleaseDate.Value;
            if (newReleaseDate.Kind == DateTimeKind.Unspecified)
            {
                movieToUpdate.ReleaseDate = DateTime.SpecifyKind(newReleaseDate, DateTimeKind.Utc);
            }
            else if (newReleaseDate.Kind == DateTimeKind.Local)
            {
                movieToUpdate.ReleaseDate = newReleaseDate.ToUniversalTime();
            }
            else
            {
                movieToUpdate.ReleaseDate = newReleaseDate;
            }
        }
        if (movieUpdateDto.DirectorId.HasValue && movieUpdateDto.DirectorId.Value != 0 && movieToUpdate.DirectorId != movieUpdateDto.DirectorId.Value)
        {
            var directorExists = await _context.Directors.AnyAsync(d => d.Id == movieUpdateDto.DirectorId.Value);
            if (!directorExists)
            {
                _logger.LogWarning($"Director with ID {movieUpdateDto.DirectorId.Value} not found. Cannot update director for movie {id}.");
                throw new ArgumentException($"Director with ID {movieUpdateDto.DirectorId.Value} not found.", nameof(movieUpdateDto.DirectorId));
            }
            movieToUpdate.DirectorId = movieUpdateDto.DirectorId.Value;
            _logger.LogInformation($"Director for movie with ID {id} updated to Director ID {movieToUpdate.DirectorId}.");
        }
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Movie with ID {id} updated successfully.");
    }
}