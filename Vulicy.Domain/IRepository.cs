namespace Vulicy.Domain;

public interface IRepository<T>
    where T : Entity
{
    Task<T?> GetById(int id);
    Task<T?> GetByIdTracked(int id);
    Task<List<T>> GetAll();
    void Add(T entity);
    void Remove(T lesson);
    Task SaveChanges();
}