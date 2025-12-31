using Vulicy.Domain;

namespace Vulicy.DB;

public class ImportRepository(VulicyDbContext dbContext)
    : RepositoryBase<ImportEntity, int>(dbContext)
        , IImportRepository
{
}