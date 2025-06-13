
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
    public async Task<Movie> CreateMovieAsync(Movie movie)
    {   
        if(movie == null)
            throw new ArgumentNullException(nameof(movie));

        //-----
        if (movie.ReleaseDate.Kind == DateTimeKind.Unspecified)
        {
            movie.ReleaseDate = DateTime.SpecifyKind(movie.ReleaseDate, DateTimeKind.Utc);
        }
        else if (movie.ReleaseDate.Kind == DateTimeKind.Local)
        {
            movie.ReleaseDate = movie.ReleaseDate.ToUniversalTime();
        }
    
        await _movieRepository.InsertAsync(movie);
    
        await _context.SaveChangesAsync();
        return movie;
    }

    public async Task<bool> DeleteMovieAsync(int id)
    {
        var movieToDelete = await _movieRepository.GetByIdAsync(id);
        if (movieToDelete == null)
        {
            _logger.LogWarning($"Movie with id {id} not found for deletion.");
            return false;
        }

        try
        {
            await _movieRepository.DeleteAsync(id);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Movie with id {id} deleted successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting entity with id {id}.");
            throw;
        }
    }

    public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
    {
        var movies = await _movieRepository.GetAllAsync();
        return movies;
    }

    public async Task<Movie?> GetMovieByIdAsync(int id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        return movie;
    }

    public async Task UpdateMovieAsync(int id, Movie movie)
    {

        var movieToUpdate = await _movieRepository.GetByIdAsync(id);
        if (movieToUpdate == null)
        {
            _logger.LogWarning($"Movie with id {id} not found for update.");
            throw new KeyNotFoundException($"Movie with id {id} not found.");
        }
        movieToUpdate.Description = movie.Description;
        movieToUpdate.DirectorId = movie.DirectorId;
        movieToUpdate.Genre = movie.Genre;
    
        // ReleaseDate'i UTC'ye çevir
        if (movie.ReleaseDate.Kind == DateTimeKind.Unspecified)
        {
            movieToUpdate.ReleaseDate = DateTime.SpecifyKind(movie.ReleaseDate, DateTimeKind.Utc);
        }
        else if (movie.ReleaseDate.Kind == DateTimeKind.Local)
        {
            movieToUpdate.ReleaseDate = movie.ReleaseDate.ToUniversalTime();
        }
        else
        {
            movieToUpdate.ReleaseDate = movie.ReleaseDate;
        }
        
        movieToUpdate.Title = movie.Title;
    
        await _movieRepository.UpdateAsync(movieToUpdate);
        await _context.SaveChangesAsync();
    }
}