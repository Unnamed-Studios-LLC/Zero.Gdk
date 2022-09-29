namespace Zero.Game.Shared
{
    internal struct ByteBuffer
    {
        public byte[] Data;
        public int Count;

        public ByteBuffer(byte[] data, int count)
        {
            Data = data;
            Count = count;
        }
    }
}
