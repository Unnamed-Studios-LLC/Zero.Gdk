namespace Zero.Game.Shared
{
    public interface IMessageHandler
    {
        void HandleEntity(uint entityId);
        void HandleWorld(uint worldId);
        void PostHandle();
        void PreHandle(uint time);
        void RemoveEntity(uint entityId);
    }
}
