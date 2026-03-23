using Microsoft.EntityFrameworkCore;
using Vulicy.Domain;

namespace Vulicy.DB;

public class HistoricRepositoryBase<T, TKey>(VulicyDbContext dbContext) : IHistoricRepository<T, TKey>
    where T : Entity<TKey>, IHistoricEntity<TKey>
{
    protected VulicyDbContext Context { get; } = dbContext;
    protected IQueryable<T> Entities => Context.Set<T>();

    public Task<List<T>> GetById(TKey id)
    {
        return Entities.Where(x => x.Id!.Equals(id)).ToListAsync();
    }

    public Task<List<T>> GetAll()
    {
        return Entities.ToListAsync();
    }

    public void Add(T entity)
    {
        Context.Add(entity);
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