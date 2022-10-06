namespace Zero.Game.Model
{
    public enum WorldFailReason
    {
        InternalError,
        LoadReturnedFalse,
        LoadThrewException,
        WorldIdTaken,
        WorkerLimitReached,
        OnStartWorldException
    }
}
