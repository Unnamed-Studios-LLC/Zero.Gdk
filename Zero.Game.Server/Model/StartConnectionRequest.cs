using System.Collections.Generic;

namespace Zero.Game.Server
{
    public class StartConnectionRequest
    {
        public StartConnectionRequest()
        {

        }

        public StartConnectionRequest(uint worldId, string clientIp, Dictionary<string, string> data)
        {
            WorldId = worldId;
            ClientIp = clientIp;
            Data = data;
        }

        public uint WorldId { get; set; }
        public string ClientIp { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }
}
