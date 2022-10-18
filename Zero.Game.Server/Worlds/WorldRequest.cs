using System.Collections.Generic;
using System.Threading.Tasks;
using Zero.Game.Model;

namespace Zero.Game.Server
{
    internal class WorldRequest
    {
        private WorldRequest(bool remove, uint worldId, Dictionary<string, string> data, TaskCompletionSource<StartWorldResponse> completed)
        {
            Remove = remove;
            WorldId = worldId;
            Data = data;
            Completed = completed;
        }

        public bool Remove { get; }
        public uint WorldId { get; }
        public Dictionary<string, string> Data { get; }
        public TaskCompletionSource<StartWorldResponse> Completed { get; }

        public static WorldRequest CreateAdd(uint worldId, Dictionary<string, string> data)
        {
            return new WorldRequest(false, worldId, data, new TaskCompletionSource<StartWorldResponse>());
        }

        public static WorldRequest CreateRemove(uint worldId)
        {
            return new WorldRequest(true, worldId, null, new TaskCompletionSource<StartWorldResponse>());
        }
    }
}
