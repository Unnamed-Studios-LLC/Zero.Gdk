using System.Collections.Generic;

namespace Zero.Game.Common
{
    public class StartWorldRequest
    {
        public StartWorldRequest()
        {

        }

        public StartWorldRequest(uint worldId, Dictionary<string, string> data)
        {
            WorldId = worldId;
            Data = data;
        }

        public StartWorldRequest(uint worldId, Dictionary<string, string> data, bool dedicatedWorker)
        {
            WorldId = worldId;
            Data = data;
            DedicatedWorker = dedicatedWorker;
        }

        public uint WorldId { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public bool DedicatedWorker { get; set; }
    }
}
