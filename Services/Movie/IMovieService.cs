public interface IMovieService
{
    Task<IEnumerable<Movie>> GetAllMoviesAsync();
    Task<Movie?> GetMovieByIdAsync(int id);
    Task<Movie> CreateMovieAsync(Movie movie);
    Task UpdateMovieAsync(int id, Movie movie);
    Task<bool> DeleteMovieAsync(int id);
}