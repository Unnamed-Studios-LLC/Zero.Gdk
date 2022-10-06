namespace Zero.Game.Server
{
    public readonly struct WorldInfo
    {
        public readonly uint Id;
        public readonly int ConnectionCount;

        public WorldInfo(uint id, int connectionCount)
        {
            Id = id;
            ConnectionCount = connectionCount;
        }
    }
}
