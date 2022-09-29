using System.Runtime.InteropServices;

namespace Zero.Game.Shared.Messaging
{
    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public readonly struct TransferMessage
    {
        [FieldOffset(0)]
        public readonly int Port;
        [FieldOffset(4)]
        public readonly long Ip;
    }
}
