using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/movies")]
[Authorize] //only authorized persons can access this endpoint
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly ILogger<MoviesController> _logger;
    public MoviesController(IMovieService movieService, ILogger<MoviesController> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Movie>>> GetAllMoviesAsync()
    {
        try
        {
            var movies = await _movieService.GetAllMoviesAsync();
            return Ok(movies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to retrieve all movies.");
            return StatusCode(500, "An unexpected error occurred while processing your request to get all movies.");
        }
    }

    [HttpGet("{id:int}", Name = "GetMovieById")]
    public async Task<ActionResult<Movie>> GetMovieByIdAsync(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid movie ID.");
        try
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null)
                return NotFound();

            return Ok(movie);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while trying to retrieve movie with id {id}.");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Movie>> CreateMovieAsync([FromBody] Movie movie)
    {
        if (movie == null)
            return BadRequest("Movie data cannot be null!");
            
        if (movie.Id != 0)
            return BadRequest("Movie ID should not be set when creating a new movie.");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdMovie = await _movieService.CreateMovieAsync(movie);
            return CreatedAtRoute("GetMovieById", new { id = createdMovie.Id }, createdMovie);
        }
        catch (ArgumentException argEx) 
        {
            _logger.LogWarning(argEx, $"Validation or business rule error while creating movie: {movie?.Title}");
            return BadRequest(argEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An unexpected error occurred while creating movie. Attempted title: {movie?.Title}");
            return StatusCode(500, "An internal error occurred while creating the movie.");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateMovieAsync(int id, [FromBody] Movie movie)
    {
        if (id <= 0) // Bu kontrol eklenebilir
            return BadRequest("Invalid movie ID.");

        if (movie == null)
            return BadRequest("Movie data cannot be null!");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        if (id != movie.Id)
            return BadRequest("The ID in the URL does not match the ID in the request body.");

        try
        {
            await _movieService.UpdateMovieAsync(id, movie);
            return NoContent();
        }
        catch (KeyNotFoundException knfex)
        {
            _logger.LogWarning(knfex, $"Movie with id {id} not found for update.");
            return NotFound(knfex.Message);
        }
        catch (ArgumentException argex)
        {
            _logger.LogWarning(argex, "Invalid data for updating movie.");
            return BadRequest(argex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating movie with id {id}");
            return StatusCode(500, "An internal error occurred while updating the movie.");
        }

    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteMovieAsync(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid movie ID.");
        try
        {
            var wasDeleted = await _movieService.DeleteMovieAsync(id);
            if (wasDeleted)
                return NoContent();
            else
                return NotFound($"Movie with id {id} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting movie with id {id}");
            return StatusCode(500, "An internal error occurred while deleting the movie.");
        }
    }
}