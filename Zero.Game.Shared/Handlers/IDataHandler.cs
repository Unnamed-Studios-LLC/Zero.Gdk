namespace Zero.Game.Shared
{
    public interface IDataHandler<T> where T : unmanaged
    {
        void HandleData(ref T data);
    }
}