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
    public async Task<ActionResult<IEnumerable<MovieDto>>> GetAllMoviesAsync()
    {
        try
        {
            var movieDtos = await _movieService.GetAllMoviesAsync();
            return Ok(movieDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to retrieve all movies.");
            return StatusCode(500, "An unexpected error occurred while processing your request to get all movies.");
        }
    }

    [HttpGet("{id:int}", Name = "GetMovieById")]
    public async Task<ActionResult<MovieDto>> GetMovieByIdAsync(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid movie ID.");
        try
        {
            var movieDto = await _movieService.GetMovieByIdAsync(id);
            if (movieDto == null)
                return NotFound();

            return Ok(movieDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while trying to retrieve movie with id {id}.");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<MovieDto>> CreateMovieAsync([FromBody] MovieCreateDto movieCreateDto)
    {
        if (movieCreateDto == null)
            return BadRequest("Movie data cannot be null!");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdMovieDto = await _movieService.CreateMovieAsync(movieCreateDto);
            return CreatedAtRoute("GetMovieById", new { id = createdMovieDto.Id }, createdMovieDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, $"Error creating movie: {ex.Message}");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An unexpected error occurred while creating movie. Attempted title: {movieCreateDto?.Title}");
            return StatusCode(500, "An internal error occurred while creating the movie.");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateMovieAsync(int id, [FromBody] MovieUpdateDto movieUpdateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _movieService.UpdateMovieAsync(id, movieUpdateDto);
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