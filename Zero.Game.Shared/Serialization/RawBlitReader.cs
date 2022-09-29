using System;
using System.Runtime.CompilerServices;

namespace Zero.Game.Shared
{
    internal unsafe struct RawBlitReader
    {
        private readonly byte* _buffer;
        private int _count;

        public RawBlitReader(byte* buffer, int capacity)
        {
            _buffer = buffer;
            Capacity = capacity;
            _count = 0;
            Faults = FaultCodes.None;
        }

        public int BytesRead => _count;
        public int Capacity { get; }
        public FaultCodes Faults { get; private set; }
        public bool IsFaulted => Faults != FaultCodes.None;

        public void Read<T>(T* destinationPointer, int length) where T : unmanaged
        {
            if (!CanContinue(sizeof(T) * length))
            {
                return;
            }

            var sourcePointer = (T*)(_buffer + _count);
            Buffer.MemoryCopy(sourcePointer, destinationPointer, sizeof(T) * length, sizeof(T) * length);
            _count += sizeof(T) * length;

            if (BitConverter.IsLittleEndian ||
                sizeof(T) == 1)
            {
                return;
            }

            for (int i = 0; i < length; i++)
            {
                EndianBlit<T>.SwapBytes((byte*)(destinationPointer + i));
            }
        }

        public bool Read<T>(T** value) where T : unmanaged
        {
            if (!CanContinue(sizeof(T)))
            {
                return false;
            }

            var pntr = (T*)(_buffer + _count);
            *value = pntr;
            _count += sizeof(T);
            return true;
        }

        public void Seek(int offset)
        {
            _count = offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanContinue(int length)
        {
            Faults |= (_count + length) > Capacity ? FaultCodes.CapacityExceeded : FaultCodes.None;
            return Faults == FaultCodes.None;
        }
    }
}
