using System.Runtime.InteropServices;

namespace Zero.Game.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 10)]
    internal readonly struct ClientBatchMessage
    {
        [FieldOffset(0)]
        public readonly uint WorldId;
        [FieldOffset(4)]
        public readonly ushort BatchId;
        [FieldOffset(6)]
        public readonly uint Time;

        public ClientBatchMessage(ulong batchKey, uint time)
        {
            WorldId = (uint)(batchKey >> 32);
            BatchId = (ushort)batchKey;
            Time = time;
        }

        public ulong BatchKey => ((ulong)WorldId << 32) | BatchId;
    }
}
