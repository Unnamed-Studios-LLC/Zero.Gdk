namespace Zero.Game.Shared
{

    internal abstract class DataDefinition
    {
        public abstract void ApplyHash(ref long hash);
        public abstract DataHandler GetHandler();
    }

    internal sealed class DataDefinition<T> : DataDefinition where T : unmanaged
    {
        public override void ApplyHash(ref long hash)
        {
            DataHash<T>.ApplyHashElement(typeof(T), ref hash);
        }

        public override DataHandler GetHandler()
        {
            return new DataHandler<T>();
        }
    }
}
