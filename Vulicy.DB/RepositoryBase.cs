using Vulicy.Domain;

namespace Vulicy.DB;

public class RepositoryBase<T> : IRepository<T>
    where T : Entity
{
    public Task<T?> GetById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetByIdTracked(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> GetAll()
    {
        throw new NotImplementedException();
    }

    public void Add(T entity)
    {
        throw new NotImplementedException();
    }

    public void Remove(T lesson)
    {
        throw new NotImplementedException();
    }

    public Task SaveChanges()
    {
        throw new NotImplementedException();
    }
}