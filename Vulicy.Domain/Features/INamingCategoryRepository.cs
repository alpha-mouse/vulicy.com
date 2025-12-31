namespace Vulicy.Domain;

public interface INamingCategoryRepository : IRepository<NamingCategoryEntity, int>
{
    Task<bool> HasAny();
    Task MergeFromCadastreInitial(DateTime now);
}