using Microsoft.EntityFrameworkCore;
using Vulicy.Domain;

namespace Vulicy.DB;

public class DossierRecordMergeSuggestionRepository(VulicyDbContext dbContext)
    : RepositoryBase<DossierRecordMergeSuggestionEntity, int>(dbContext)
        , IDossierRecordMergeSuggestionRepository
{
    public Task<DossierRecordMergeSuggestion?> GetNext()
    {
        return Entities
            .OrderBy(x => x.Id)
            .Select(x => new DossierRecordMergeSuggestion(
                x.Id,
                new DossierRecordSearchResult(x.LeftRecord.Id, x.LeftRecord.NameBeTarask, x.LeftRecord.NameBeNark, x.LeftRecord.NameRu, x.LeftRecord.DescriptionBe, x.LeftRecord.DescriptionRu, x.LeftRecord.Classification, x.LeftRecord.NamingCategoryId, 0),
                new DossierRecordSearchResult(x.RightRecord.Id, x.RightRecord.NameBeTarask, x.RightRecord.NameBeNark, x.RightRecord.NameRu, x.RightRecord.DescriptionBe, x.RightRecord.DescriptionRu, x.RightRecord.Classification, x.RightRecord.NamingCategoryId, 0)
            ))
            .FirstOrDefaultAsync();
    }

    public void Delete(DossierRecordMergeSuggestionEntity suggestion)
    {
        Context.Remove(suggestion);
    }
}