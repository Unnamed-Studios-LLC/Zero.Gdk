namespace Zero.Game.Server
{
    public interface IRemoveEvent<T> where T : unmanaged
    {
        internal static int Type = TypeCache<T>.Type;
        void OnRemove(uint entityId, in T component);
    }
}
