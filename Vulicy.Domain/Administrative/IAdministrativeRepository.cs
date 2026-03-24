using NetTopologySuite.Geometries;

namespace Vulicy.Domain;

public interface IAdministrativeRepository : IRepository<AdministrativeEntity, int>
{
    Task CreateMissingAdministrativeFromCadastre();
    Task SetAdministrativeOnFeatures();
    Task<AdministrativeEntity?> GetByCadastreAte(int cadastreAte);
    Task<List<AdministrativeEntity>> GetBatchTracking(int greaterThanId, int take);
    Task<AdministrativeEntity?> FindIntersecting(Geometry geometry);
}