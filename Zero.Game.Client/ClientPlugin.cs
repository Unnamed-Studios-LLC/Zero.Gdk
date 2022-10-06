using Zero.Game.Shared;

namespace Zero.Game.Client
{
    public abstract class ClientPlugin
    {
        public ClientOptions Options { get; } = new ClientOptions();

        public abstract void BuildData(DataBuilder builder);
        public virtual void Connected() { }
        public virtual void Connecting() { }
        public virtual void Disconnected() { }
    }
}
