using Microsoft.EntityFrameworkCore;
using Vulicy.Domain;

namespace Vulicy.DB;

public class RepositoryBase<T, TKey>(VulicyDbContext dbContext) : IRepository<T, TKey>
    where T : Entity<TKey>
{
    protected VulicyDbContext Context { get; } = dbContext;
    protected IQueryable<T> Entities => Context.Set<T>();

    public Task<T?> GetById(TKey id)
    {
        return Entities.FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    public Task<T?> GetByIdTracked(TKey id)
    {
        return Entities.AsTracking().FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    public Task<List<T>> GetAll()
    {
        return Entities.ToListAsync();
    }

    public void Add(T entity)
    {
        Context.Add(entity);
    }

    public void AddRange(IEnumerable<T> entities)
    {
        Context.AddRange(entities);
    }

    public Task SaveChanges()
    {
        return Context.SaveChangesAsync();
    }

    public void ClearChangeTracker()
    {
        Context.ChangeTracker.Clear();
    }
}