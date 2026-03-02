namespace Vulicy.Domain;

public interface IAdministrativeRepository : IRepository<AdministrativeEntity, int>
{
    Task CreateMissingAdministrativeFromCadastre();
    Task SetAdministrativeOnFeatures();
}