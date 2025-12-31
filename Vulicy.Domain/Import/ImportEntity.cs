namespace Vulicy.Domain;

public class ImportEntity : Entity<int>
{
    public required string DownloadUrl { get; init; }
    public required string LocalPath { get; init; }
    public bool Cleared { get; set; }
    public ImportType Type { get; set; }
    public ImportStatus Status { get; set; }
    public string? Error { get; set; }
}