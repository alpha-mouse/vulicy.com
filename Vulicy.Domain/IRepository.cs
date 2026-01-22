using System.Data.Common;

namespace Vulicy.Domain;

public interface IRepository<T, in TKey>
    where T : Entity<TKey>
{
    Task<T?> GetById(TKey id);
    Task<T?> GetByIdTracked(TKey id);
    Task<List<T>> GetAll();
    void Add(T entity);
    void AddRange(IEnumerable<T> entities);
    Task SaveChanges();
    void ClearChangeTracker();
    Task<ITransaction> BeginTransaction();
}

public interface ITransaction : IDisposable, IAsyncDisposable
{
    Task Commit();
}