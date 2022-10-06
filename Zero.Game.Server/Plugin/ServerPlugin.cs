using System.Threading.Tasks;
using Zero.Game.Model;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public abstract class ServerPlugin
    {
        private static readonly Task<bool> s_completedLoadTask = Task.FromResult(true);

        public ServerOptions Options { get; } = new();

        public virtual void AddToWorld(Connection connection) { }
        public virtual void BuildData(DataBuilder builder) { }
        public virtual Task DeferredWorldResponseAsync(StartWorldResponse response) => Task.CompletedTask;
        public virtual Task<bool> LoadConnectionAsync(Connection connection) => s_completedLoadTask;
        public virtual Task<bool> LoadWorldAsync(World world) => s_completedLoadTask;
        public virtual Task OnStartConnectionAsync(StartConnectionRequest request) => Task.CompletedTask;
        public virtual Task OnStartWorldAsync(StartWorldRequest request) => Task.CompletedTask;
        public virtual Task OnStopWorldAsync(uint worldId) => Task.CompletedTask;
        public virtual void RemoveFromWorld(Connection connection) { }
        public virtual Task StartDeploymentAsync() => Task.CompletedTask;
        public virtual Task StartWorkerAsync() => Task.CompletedTask;
        public virtual Task StopDeploymentAsync() => Task.CompletedTask;
        public virtual Task StopWorkerAsync() => Task.CompletedTask;
        public virtual Task UnloadConnectionAsync(Connection connection) => s_completedLoadTask;
        public virtual Task UnloadWorldAsync(World world) => s_completedLoadTask;
    }
}