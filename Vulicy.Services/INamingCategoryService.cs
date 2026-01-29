using Vulicy.Domain;

namespace Vulicy.Services;

public interface INamingCategoryService
{
    Task<List<NamingCategory>> GetAll();
}

public class NamingCategoryService(INamingCategoryRepository namingCategoryRepository) : INamingCategoryService
{
    public async Task<List<NamingCategory>> GetAll()
    {
        var entities = await namingCategoryRepository.GetAll();
        return entities
            .Select(x => new NamingCategory(x.Id, x.Name))
            .ToList();
    }
}

public record NamingCategory(int Id, string Name);