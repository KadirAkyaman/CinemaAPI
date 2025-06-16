public interface IMovieService
{
    Task<IEnumerable<MovieDto>> GetAllMoviesAsync();
    Task<MovieDto?> GetMovieByIdAsync(int id);
    Task<MovieDto> CreateMovieAsync(MovieCreateDto movieCreateDto);
    Task UpdateMovieAsync(int id, MovieUpdateDto movieUpdateDto);
    Task<bool> DeleteMovieAsync(int id);
}