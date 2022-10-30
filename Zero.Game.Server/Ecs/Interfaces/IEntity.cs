namespace Zero.Game.Server
{
    public interface IEntity
    {
        Entities Entities { get; }
        uint EntityId { get; }
    }
}
