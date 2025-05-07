namespace Quivi.Domain.Entities.Pos
{
    public enum SyncState : int
    {
        SyncFailure = -3,
        PoSOffline = -2,
        Stopped = -1,
        Unknown = 0,
        Running = 1,
    }
}
