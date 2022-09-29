using System.Runtime.InteropServices;

namespace Zero.Game.Shared
{
    [StructLayout(LayoutKind.Explicit, Size = 6)]
    internal readonly struct EntityMessage
    {
        [FieldOffset(0)]
        public readonly uint EntityId;
        [FieldOffset(4)]
        public readonly ushort DataCount;

        public EntityMessage(uint entityId, ushort dataCount)
        {
            EntityId = entityId;
            DataCount = dataCount;
        }
    }
}
