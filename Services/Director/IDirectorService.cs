public interface IDirectorService
{
    Task<IEnumerable<DirectorDto>> GetAllDirectorsAsync();
    Task<DirectorDto?> GetDirectorByIdAsync(int id);
    Task<DirectorDto> CreateDirectorAsync(DirectorCreateDto directorCreateDto);
    Task UpdateDirectorAsync(int id, DirectorUpdateDto directorUpdateDto);
    Task<bool> DeleteDirectorAsync(int id);
}