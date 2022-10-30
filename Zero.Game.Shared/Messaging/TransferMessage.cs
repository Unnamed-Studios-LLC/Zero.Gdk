using System.Runtime.InteropServices;

namespace Zero.Game.Shared.Messaging
{
    [StructLayout(LayoutKind.Explicit, Size = Size)]
    public unsafe struct TransferMessage
    {
        public const int Size = 4 + IpLength + KeyLength;
        public const int IpLength = 16;
        public const int KeyLength = ConnectionSocket.KeyLength;

        [FieldOffset(0)]
        public ushort Port;
        [FieldOffset(2)]
        public ushort IpSize;
        [FieldOffset(4)]
        public fixed byte Ip[IpLength];
        [FieldOffset(4 + IpLength)]
        public fixed byte Key[KeyLength];
    }
}
