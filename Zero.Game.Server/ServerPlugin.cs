using System.Threading.Tasks;
using Zero.Game.Model;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public abstract class ServerPlugin
    {
        private static readonly Task<bool> s_completedLoadTask = Task.FromResult(true);

        /// <summary>
        /// server options
        /// </summary>
        public ServerOptions Options { get; } = new();

        /// <summary>
        /// add the connection to it's world (connections may be added/removed from worlds without load/unload being called)
        /// </summary>
        /// <param name="connection"></param>
        public virtual void AddToWorld(Connection connection) { }

        /// <summary>
        /// build data definitions
        /// </summary>
        /// <param name="builder"></param>
        public virtual void BuildData(DataBuilder builder) { }

        /// <summary>
        /// a deferred world start has completed
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public virtual Task DeferredWorldResponseAsync(StartWorldResponse response) => Task.CompletedTask;

        /// <summary>
        /// load a connection's data (load from database, etc.)
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public virtual Task<bool> LoadConnectionAsync(Connection connection) => s_completedLoadTask;

        /// <summary>
        /// load a world's data (database, files, etc.)
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public virtual Task<bool> LoadWorldAsync(World world) => s_completedLoadTask;

        /// <summary>
        /// Called before a connection has started. The entire given request may be altered by the method. This method is called by the deployment instance
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual Task OnStartConnectionAsync(StartConnectionRequest request) => Task.CompletedTask;

        /// <summary>
        /// Called before a world has started. WorldId CANNOT be altered, however, the rest of the request may be altered by the method. This method is called by the deployment instance
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual Task OnStartWorldAsync(StartWorldRequest request) => Task.CompletedTask;

        /// <summary>
        /// Called before a world has stopped. This method is called by the deployment instance
        /// </summary>
        /// <param name="worldId"></param>
        /// <returns></returns>
        public virtual Task OnStopWorldAsync(uint worldId) => Task.CompletedTask;

        /// <summary>
        /// // remove a connection from a world
        /// </summary>
        /// <param name="connection"></param>
        public virtual void RemoveFromWorld(Connection connection) { }

        /// <summary>
        /// starts a server deployment, typically houses initial world generation
        /// </summary>
        /// <returns></returns>
        public virtual Task StartDeploymentAsync() => Task.CompletedTask;

        /// <summary>
        /// starts a server worker, called once per worker before any logic, typically sets up a workers environment
        /// </summary>
        /// <returns></returns>
        public virtual Task StartWorkerAsync() => Task.CompletedTask;

        /// <summary>
        /// called when a deployment has stopped
        /// </summary>
        /// <returns></returns>
        public virtual Task StopDeploymentAsync() => Task.CompletedTask;

        /// <summary>
        /// called when a worker has stopped
        /// </summary>
        /// <returns></returns>
        public virtual Task StopWorkerAsync() => Task.CompletedTask;

        /// <summary>
        /// unloads a connection (save to database, etc.)
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public virtual Task UnloadConnectionAsync(Connection connection) => s_completedLoadTask;

        /// <summary>
        /// unloads a world
        /// </summary>
        /// <param name="world"></param>
        /// <returns></returns>
        public virtual Task UnloadWorldAsync(World world) => s_completedLoadTask;
    }
}