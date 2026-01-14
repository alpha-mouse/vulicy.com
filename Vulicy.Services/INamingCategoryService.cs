using Vulicy.Domain;

namespace Vulicy.Services;

public interface INamingCategoryService
{
    Task<List<NamingCategoryDto>> GetAll();
}

public class NamingCategoryService(INamingCategoryRepository namingCategoryRepository) : INamingCategoryService
{
    public async Task<List<NamingCategoryDto>> GetAll()
    {
        var entities = await namingCategoryRepository.GetAll();
        return entities
            .Select(x => new NamingCategoryDto(x.Id, x.Name))
            .ToList();
    }
}

public record NamingCategoryDto(int Id, string Name);