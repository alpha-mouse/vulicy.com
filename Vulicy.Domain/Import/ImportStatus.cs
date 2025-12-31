namespace Vulicy.Domain;

public enum ImportStatus
{
    Pending = 0,
    Downloaded = 10,
    Staged = 20,
    Complete = 100,
    DownloadFailed = 210,
    StagingFailed = 220,
    Failed = 300,
}