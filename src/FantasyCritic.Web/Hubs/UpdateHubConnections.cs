namespace FantasyCritic.Web.Hubs;

/// <summary>
/// How many browsers are connected to <see cref="UpdateHub"/> right now, for the admin monitor. The league page only connects
/// while its draft is active, so this is how many are watching a live draft. In this process only.
/// </summary>
public class UpdateHubConnections
{
    private int _count;

    public int Count => Volatile.Read(ref _count);

    public void Connected() => Interlocked.Increment(ref _count);
    public void Disconnected() => Interlocked.Decrement(ref _count);
}
