using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/directors")]
public class DirectorsController : ControllerBase
{
    private readonly IDirectorService _directorService;
    private readonly ILogger<DirectorsController> _logger;
    public DirectorsController(IDirectorService directorService, ILogger<DirectorsController> logger)
    {
        _directorService = directorService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Director>>> GetAllDirectorsAsync()
    {
        try
        {
            var directors = await _directorService.GetAllDirectorsAsync();
            return Ok(directors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while trying to retrieve all directors.");
            return StatusCode(500, "An unexpected error occurred while processing your request to get all directors.");
        }
    }

    [HttpGet("{id:int}", Name = "GetDirectorById")]
    public async Task<ActionResult<Director>> GetDirectorByIdAsync(int id) 
    {
        if (id <= 0)
            return BadRequest("Invalid director ID.");
        try
        {
            var director = await _directorService.GetDirectorByIdAsync(id); 
            if (director == null)
                return NotFound();

            return Ok(director);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while trying to retrieve director with id {id}.");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Director>> CreateDirectorAsync([FromBody] Director director)
    {
        if (director == null)
            return BadRequest("Director data cannot be null!");

        if (director.Id != 0)
            return BadRequest("Director ID should not be set when creating a new director.");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdDirector = await _directorService.CreateDirectorAsync(director);

            return CreatedAtRoute("GetDirectorById", new { id = createdDirector.Id }, createdDirector);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, $"Validation or business rule error while creating director: {director?.Name} {director?.Surname}");
            return BadRequest(argEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An unexpected error occurred while creating director. {director?.Name} {director?.Surname}");
            return StatusCode(500, "An internal error occurred while creating the director.");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateDirectorAsync(int id, [FromBody] Director director)
    {
        if (id <= 0) // Bu kontrol eklenebilir
            return BadRequest("Invalid director ID.");

        if (director == null)
            return BadRequest("Director data cannot be null!");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != director.Id)
            return BadRequest("The ID in the URL does not match the ID in the request body.");

        try
        {
            await _directorService.UpdateDirectorAsync(id, director);
            return NoContent();
        }
        catch (KeyNotFoundException knfex)
        {
            _logger.LogWarning(knfex, $"Director with id {id} not found for update.");
            return NotFound(knfex.Message);
        }
        catch (ArgumentException argex)
        {
            _logger.LogWarning(argex, "Invalid data for updating director.");
            return BadRequest(argex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating director with id {id}");
            return StatusCode(500, "An internal error occurred while updating the director.");
        }
    }
    
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteDirectorAsync(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid director ID.");
        try
        {
            var wasDeleted = await _directorService.DeleteDirectorAsync(id);
            if (wasDeleted)
                return NoContent();
            else
                return NotFound($"Director with id {id} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting director with id {id}");
            return StatusCode(500, "An internal error occurred while deleting the director.");
        }
    }
}