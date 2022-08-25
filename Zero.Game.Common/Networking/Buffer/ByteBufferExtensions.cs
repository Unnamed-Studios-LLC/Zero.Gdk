namespace Zero.Game.Common
{
    public static class ByteBufferExtensions
    {
        public static ByteBuffer SetSize(this ByteBuffer buffer, int size)
        {
            return new ByteBuffer(buffer.Data, size);
        }
    }
}
