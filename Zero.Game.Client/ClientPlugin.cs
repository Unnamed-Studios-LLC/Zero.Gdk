using Zero.Game.Shared;

namespace Zero.Game.Client
{
    public abstract class ClientPlugin
    {
        /// <summary>
        /// client options
        /// </summary>
        public ClientOptions Options { get; } = new ClientOptions();

        /// <summary>
        /// build data definitions
        /// </summary>
        /// <param name="builder"></param>
        public virtual void BuildData(DataBuilder builder) { }

        /// <summary>
        /// connected to server
        /// </summary>
        public virtual void Connected() { }

        /// <summary>
        /// connecting to server
        /// </summary>
        public virtual void Connecting() { }

        /// <summary>
        /// disconnected from server
        /// </summary>
        public virtual void Disconnected() { }

        /// <summary>
        /// update function called after receive and before send
        /// </summary>
        public virtual void Update() { }
    }
}
