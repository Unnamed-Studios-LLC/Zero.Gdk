using System.Collections.Concurrent;

namespace Zero.Game.Common
{
    public static class PacketBufferCache
    {
        public static int MaxBufferSize { get; set; } = 25_000;

        private static readonly ConcurrentQueue<byte[]> s_buffers = new ConcurrentQueue<byte[]>();

        public static ByteBuffer GetBuffer()
        {
            if (s_buffers.TryDequeue(out var buffer))
            {
                return new ByteBuffer(buffer, 0);
            }
            return new ByteBuffer(new byte[MaxBufferSize], 0);
        }

        public static void ReturnBuffer(ByteBuffer buffer)
        {
            s_buffers.Enqueue(buffer.Data);
        }
    }
}
