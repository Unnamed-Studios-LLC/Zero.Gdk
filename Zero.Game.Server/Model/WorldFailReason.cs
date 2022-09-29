namespace Zero.Game.Server
{
    public enum WorldFailReason
    {
        InternalError,
        LoadReturnedFalse,
        LoadThrewException,
        WorldIdTaken,
        WorkerLimitReached
    }
}
