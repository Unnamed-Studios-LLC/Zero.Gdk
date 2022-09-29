namespace Zero.Game.Server
{
    public enum ConnectionFailReason
    {
        InternalError,
        WorldNotFound,
        InvalidClientIpAddress,
        PerIpRateExceeded
    }
}
