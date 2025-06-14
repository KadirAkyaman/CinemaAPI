public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task InsertAsync(T Entity);
    Task Update(T entity);
    Task DeleteAsync(int id);
}