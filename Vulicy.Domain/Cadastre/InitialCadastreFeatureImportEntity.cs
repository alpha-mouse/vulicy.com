namespace Vulicy.Domain;

public class InitialCadastreFeatureImportEntity : Entity<string>
{
    public int? Classification { get; set; }
    public string? Reason { get; set; }
    public string? HistoricName { get; set; }
    public string? NameCategory { get; set; }
    public string? Comment { get; set; }
    public bool HistoricPossible { get; set; }
    public string? YearNamed { get; set; }
}