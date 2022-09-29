using System.Runtime.InteropServices;

namespace Zero.Game.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct ServerBatchMessage
    {
        [FieldOffset(0)]
        public readonly uint WorldId;
        [FieldOffset(4)]
        public readonly ushort BatchId;
        [FieldOffset(6)]
        public readonly ushort RemovedCount;

        public ServerBatchMessage(ushort batchId, uint worldId, ushort removedCount)
        {
            BatchId = batchId;
            WorldId = worldId;
            RemovedCount = removedCount;
        }

        public ulong BatchKey => ((ulong)WorldId << 32) | BatchId;
    }
}
