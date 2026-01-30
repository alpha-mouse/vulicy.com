using Vulicy.Domain;

namespace Vulicy.Services;

public interface IDossierRecordService
{
    Task<List<DossierRecordSearchResult>> SearchByName(string? query, int? skip = null, int? take = null);
    Task<int> CreateRecord(EditDossierRecordRequest request, int userId);
    Task MergeDossierRecord(int id, MergeDossierRecordRequest request, int userId);
    Task EditRecord(int id, EditDossierRecordRequest request, int userId);
    Task DeleteDossierRecord(int id, int userId);
}

public class DossierRecordService(
    IDossierRecordRepository dossierRecordRepository,
    IDossierRecordHistoricRepository dossierRecordHistoricRepository) : IDossierRecordService
{
    public Task<List<DossierRecordSearchResult>> SearchByName(string? query, int? skip = null, int? take = null)
    {
        if (skip < 0 || take < 0 || 1000 < take) throw new InvalidOperationException("Bad paging parameters");
        return dossierRecordRepository.SearchByName(query, skip ?? 0, take ?? 100);
    }

    public async Task<int> CreateRecord(EditDossierRecordRequest request, int userId)
    {
        var record = new DossierRecordEntity
        {
            NameBeTarask = request.NameBeTarask,
            NameBeNark = request.NameBeNark,
            NameRu = request.NameRu,
            DescriptionBe = request.DescriptionBe,
            DescriptionRu = request.DescriptionRu,
            Classification = request.Classification,
            LastModifiedById = userId,
        };

        dossierRecordRepository.Add(record);
        await dossierRecordRepository.SaveChanges();

        return record.Id;
    }

    public async Task MergeDossierRecord(int id, MergeDossierRecordRequest request, int userId)
    {
        await using var transaction = await dossierRecordRepository.BeginTransaction();

        // 1. Fetch both dossier records (tracked for modification)
        var canonical = await dossierRecordRepository.GetByIdTracked(id);
        var other = await dossierRecordRepository.GetByIdTracked(request.OtherId);

        if (canonical is null || other is null)
        {
            throw new InvalidOperationException("One or both dossier records not found");
        }

        // 2. Create historic records for both
        var now = DateTime.UtcNow;
        var canonicalHistoric = DossierRecordHistoricEntity.FromBase(canonical);
        canonicalHistoric.ChangeDateTime = now;
        canonicalHistoric.InHistoryById = userId;
        dossierRecordHistoricRepository.Add(canonicalHistoric);

        var otherHistoric = DossierRecordHistoricEntity.FromBase(other);
        otherHistoric.ChangeDateTime = now;
        otherHistoric.InHistoryById = userId;
        dossierRecordHistoricRepository.Add(otherHistoric);

        // 3. Update canonical record with request fields
        canonical.NameBeTarask = request.NameBeTarask;
        canonical.NameBeNark = request.NameBeNark;
        canonical.NameRu = request.NameRu;
        canonical.DescriptionBe = request.DescriptionBe;
        canonical.DescriptionRu = request.DescriptionRu;
        canonical.Classification = request.Classification;
        canonical.NamingCategoryId = request.NamingCategoryId;
        canonical.LastModifiedById = userId;

        // 4. Union PossibleNamesBeNark and PossibleNamesRu
        canonical.PossibleNamesBeNark = UnionLists(canonical.PossibleNamesBeNark, other.PossibleNamesBeNark);
        canonical.PossibleNamesRu = UnionLists(canonical.PossibleNamesRu, other.PossibleNamesRu);

        // 5. Union AlternativeDescriptions, then exclude the resulting Description
        canonical.AlternativeDescriptionsBe = UnionLists(canonical.AlternativeDescriptionsBe, other.AlternativeDescriptionsBe)
            ?.Where(d => d != canonical.DescriptionBe)
            .ToList();
        if (canonical.AlternativeDescriptionsBe?.Count == 0)
            canonical.AlternativeDescriptionsBe = null;

        canonical.AlternativeDescriptionsRu = UnionLists(canonical.AlternativeDescriptionsRu, other.AlternativeDescriptionsRu)
            ?.Where(d => d != canonical.DescriptionRu)
            .ToList();
        if (canonical.AlternativeDescriptionsRu?.Count == 0)
            canonical.AlternativeDescriptionsRu = null;

        // 6. Relink features from other dossier record to canonical
        await dossierRecordRepository.RelinkFeatures(other.Id, canonical.Id);

        // 7. Delete the other dossier record
        dossierRecordRepository.Delete(other);

        await dossierRecordRepository.SaveChanges();
        await transaction.Commit();
    }

    public async Task EditRecord(int id, EditDossierRecordRequest request, int userId)
    {
        await using var transaction = await dossierRecordRepository.BeginTransaction();

        var record = await dossierRecordRepository.GetByIdTracked(id);
        if (record is null)
        {
            throw new InvalidOperationException("Dossier record not found");
        }

        var now = DateTime.UtcNow;
        var historic = DossierRecordHistoricEntity.FromBase(record);
        historic.ChangeDateTime = now;
        historic.InHistoryById = userId;
        dossierRecordHistoricRepository.Add(historic);

        record.NameBeTarask = request.NameBeTarask;
        record.NameBeNark = request.NameBeNark;
        record.NameRu = request.NameRu;
        record.DescriptionBe = request.DescriptionBe;
        record.DescriptionRu = request.DescriptionRu;
        record.Classification = request.Classification;
        record.LastModifiedById = userId;

        await dossierRecordRepository.SaveChanges();
        await transaction.Commit();
    }

    public async Task DeleteDossierRecord(int id, int userId)
    {
        await using var transaction = await dossierRecordRepository.BeginTransaction();

        var record = await dossierRecordRepository.GetByIdTracked(id);
        if (record is null)
        {
            throw new InvalidOperationException("Dossier record not found");
        }

        if (await dossierRecordRepository.HasFeatures(id))
        {
            throw new InvalidOperationException("Cannot delete dossier record with attached features");
        }

        var now = DateTime.UtcNow;
        var historic = DossierRecordHistoricEntity.FromBase(record);
        historic.ChangeDateTime = now;
        historic.InHistoryById = userId;
        dossierRecordHistoricRepository.Add(historic);

        dossierRecordRepository.Delete(record);

        await dossierRecordRepository.SaveChanges();
        await transaction.Commit();
    }

    private static List<string>? UnionLists(List<string>? list1, List<string>? list2)
    {
        if (list1 is null && list2 is null)
            return null;

        if (list1 is null ^ list2 is null)
            return list1 ?? list2;

        var result = new HashSet<string>(list1 ?? [], StringComparer.Ordinal);
        if (list2 is not null)
        {
            foreach (var item in list2)
            {
                result.Add(item);
            }
        }

        return result.Count > 0 ? [.. result] : null;
    }
}