namespace Zero.Game.Shared
{
    public interface ISerializable
    {
        void Serialize(ISerializer serializer);
    }
}
