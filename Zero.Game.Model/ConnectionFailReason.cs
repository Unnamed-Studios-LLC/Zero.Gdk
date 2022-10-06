namespace Zero.Game.Model
{
    public enum ConnectionFailReason
    {
        InternalError,
        WorldNotFound,
        InvalidClientIpAddress,
        PerIpRateExceeded,
        OnStartConnectionException,
        MaxConnectionsReached
    }
}
