namespace Vulicy.Domain;

/// <summary>
/// Ambient marker that lets long-running import work opt out of the default DB command timeout.
/// The flag flows down the async call chain (AsyncLocal), so every query issued while an
/// <see cref="Unlimited"/> scope is active runs without a timeout; everything else keeps the default.
/// </summary>
public static class DbCommandTimeout
{
    private static readonly AsyncLocal<bool> UnlimitedFlag = new();

    public static bool IsUnlimited => UnlimitedFlag.Value;

    public static IDisposable Unlimited()
    {
        var previous = UnlimitedFlag.Value;
        UnlimitedFlag.Value = true;
        return new Scope(previous);
    }

    private sealed class Scope(bool previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            UnlimitedFlag.Value = previous;
        }
    }
}
