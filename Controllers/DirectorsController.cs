using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/directors")]
[Authorize]
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
    public async Task<ActionResult<IEnumerable<DirectorDto>>> GetAllDirectorsAsync()
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
    public async Task<ActionResult<DirectorDto>> GetDirectorByIdAsync(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid director ID.");
        try
        {
            var directorDto = await _directorService.GetDirectorByIdAsync(id);
            if (directorDto == null)
                return NotFound();

            return Ok(directorDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while trying to retrieve director with id {id}.");
            return StatusCode(500, "An unexpected error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<DirectorDto>> CreateDirectorAsync([FromBody] DirectorCreateDto directorCreateDto)
    {
        if (directorCreateDto == null)
            return BadRequest("Director data cannot be null!");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdDirectorDbo = await _directorService.CreateDirectorAsync(directorCreateDto);

            return CreatedAtRoute("GetDirectorById", new { id = createdDirectorDbo.Id }, createdDirectorDbo);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, $"Validation or business rule error while creating director: {directorCreateDto?.Name} {directorCreateDto?.Surname}");
            return BadRequest(argEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An unexpected error occurred while creating director. {directorCreateDto?.Name} {directorCreateDto?.Surname}");
            return StatusCode(500, "An internal error occurred while creating the director.");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateDirectorAsync(int id, [FromBody] DirectorUpdateDto directorUpdateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _directorService.UpdateDirectorAsync(id, directorUpdateDto);
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