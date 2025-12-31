namespace Vulicy.Domain;

public interface IHistoricRepository<T, in TKey>
    where T : Entity<TKey>, IHistoricEntity<TKey>
{
    Task<List<T>> GetById(TKey id);
    Task<List<T>> GetAll();
    void Add(T entity);
    Task SaveChanges();
    void ClearChangeTracker();
}