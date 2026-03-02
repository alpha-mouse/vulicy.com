using Vulicy.Domain;

namespace Vulicy.DB.Repositories;

public class ImportRepository(VulicyDbContext dbContext)
    : RepositoryBase<ImportEntity, int>(dbContext)
        , IImportRepository
{
}