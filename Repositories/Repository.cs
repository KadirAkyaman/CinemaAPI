using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class Repository<T> : IRepository<T> where T : class, IEntity
{
    private readonly AppDbContext _context;
    private readonly ILogger<Repository<T>> _logger;

    public Repository(AppDbContext context, ILogger<Repository<T>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var data = await _context.Set<T>().FindAsync(id);

            if (data != null)
                _context.Set<T>().Remove(data);


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting entity with id {id}.");
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            var datas = await _context.Set<T>().ToListAsync();
            return datas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving all entities.");
            throw;
        }
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            var data = await _context.Set<T>().SingleOrDefaultAsync(d => d.Id == id);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving the entity with id {id}.");
            throw;
        }
    }

    public async Task InsertAsync(T entity)
    {
        try
        {
            await _context.Set<T>().AddAsync(entity);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while inserting a new entity.");
            throw;
        }
    }

    public Task Update(T entity)
    {
        try
        {
            _context.Set<T>().Update(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in Repository.Update for entity id {entity.Id}.");
            throw;
        }
        return Task.CompletedTask;
    }
}
