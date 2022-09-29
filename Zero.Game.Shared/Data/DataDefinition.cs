namespace Zero.Game.Shared
{

    internal abstract class DataDefinition
    {
        public abstract DataHandler GetHandler();
    }

    internal sealed class DataDefinition<T> : DataDefinition where T : unmanaged
    {
        public override DataHandler GetHandler()
        {
            return new DataHandler<T>();
        }
    }
}
