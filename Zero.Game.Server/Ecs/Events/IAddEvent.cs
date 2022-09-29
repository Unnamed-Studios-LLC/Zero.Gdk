namespace Zero.Game.Server
{
    public interface IAddEvent<T> where T : unmanaged
    {
        internal static int Type = TypeCache<T>.Type;
        void OnAdd(uint entityId, ref T component);
    }
}