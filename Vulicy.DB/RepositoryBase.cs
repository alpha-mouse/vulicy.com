using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Vulicy.Domain;

namespace Vulicy.DB;

public partial class RepositoryBase<T, TKey>(VulicyDbContext dbContext) : IRepository<T, TKey>
    where T : Entity<TKey>
{
    protected VulicyDbContext Context { get; } = dbContext;
    protected IQueryable<T> Entities => Context.Set<T>();

    public Task<T?> GetById(TKey id)
    {
        return Entities.FirstOrDefaultAsync(x => x.Id!.Equals(id));
    }

    public Task<T?> GetByIdTracked(TKey id)
    {
        return Entities.AsTracking().FirstOrDefaultAsync(x => x.Id!.Equals(id));
    }

    public Task<bool> Exists(TKey id)
    {
        return Entities.AnyAsync(x => x.Id!.Equals(id));
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

    public async Task<ITransaction> BeginTransaction()
    {
        return new Transaction(await Context.Database.BeginTransactionAsync());
    }

    private record Transaction(IDbContextTransaction DbContextTransaction) : ITransaction
    {
        public Task Commit() => DbContextTransaction.CommitAsync();

        public void Dispose() => DbContextTransaction.Dispose();

        public ValueTask DisposeAsync() => DbContextTransaction.DisposeAsync();
    }
}