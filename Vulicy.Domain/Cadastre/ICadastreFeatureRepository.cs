namespace Vulicy.Domain;

public interface ICadastreFeatureRepository : IRepository<CadastreFeatureEntity, string>
{
    Task<byte[]?> GetTile(int z, int x, int y);
    Task<List<int>> GetUnmatchedAtes();
    Task<List<CadastreFeatureEntity>> GetUnmatchedByAteTracking(int ate);
    Task<List<int>> GetAllAtes();
    Task<List<CadastreFeatureSearchResult>> SearchUnmatchedByName(string query, double? lat = null, double? lng = null);
}