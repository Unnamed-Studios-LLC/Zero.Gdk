namespace Zero.Game.Model
{
    public class StartWorldResponse
    {
        public StartWorldResponse()
        {

        }

        public StartWorldResponse(uint worldId)
        {
            WorldId = worldId;
            State = WorldStartState.Started;
        }

        public StartWorldResponse(uint worldId, WorldStartState state)
        {
            WorldId = worldId;
            State = state;
        }

        public StartWorldResponse(WorldFailReason failReason)
        {
            State = WorldStartState.Failed;
            FailReason = failReason;
        }

        public uint WorldId { get; set; }
        public WorldStartState State { get; set; }
        public WorldFailReason? FailReason { get; set; }


        public static implicit operator StartWorldResponse(WorldFailReason failReason) => new StartWorldResponse(failReason);
    }
}
