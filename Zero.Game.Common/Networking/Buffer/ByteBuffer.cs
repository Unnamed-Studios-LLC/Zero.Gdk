namespace Zero.Game.Common
{
    public struct ByteBuffer
    {
        public int Capacity => Data.Length;

        public byte[] Data { get; }

        public int Size { get; }

        public ByteBuffer(byte[] data, int size)
        {
            Data = data;
            Size = size;
        }
    }
}
