using System;
using System.Runtime.CompilerServices;

namespace Zero.Game.Shared
{
    internal unsafe struct BlitWriter
    {
        private readonly byte* _buffer;
        private readonly int _capacity;
        private int _count;

        public BlitWriter(byte* buffer, int capacity)
        {
            _buffer = buffer;
            _capacity = capacity;
            _count = 0;
            Faults = FaultCodes.None;
        }

        public int BytesWritten => _count;
        public FaultCodes Faults { get; private set; }
        public bool IsFaulted => Faults != FaultCodes.None;

        public void Seek(int offset)
        {
            _count = offset;
        }

        public bool Write<T>(T* sourcePointer, int length) where T : unmanaged
        {
            if (!CanContinue(sizeof(T) * length))
            {
                return false;
            }

            var destinationPointer = (T*)(_buffer + _count);
            Buffer.MemoryCopy(sourcePointer, destinationPointer, _capacity - _count, sizeof(T) * length);
            _count += sizeof(T) * length;

            if (!BitConverter.IsLittleEndian && sizeof(T) != 1)
            {
                for (int i = 0; i < length; i++)
                {
                    EndianBlit<T>.SwapBytes((byte*)(destinationPointer + i));
                }
            }

            return true;
        }

        public bool Write<T>(T value) where T : unmanaged
        {
            return Write(&value);
        }

        public bool Write<T>(T* value) where T : unmanaged
        {
            if (!CanContinue(sizeof(T)))
            {
                return false;
            }

            var pntr = (T*)(_buffer + _count);
            *pntr = *value;
            _count += sizeof(T);

            if (!BitConverter.IsLittleEndian && sizeof(T) != 1)
            {
                EndianBlit<T>.SwapBytes((byte*)pntr);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanContinue(int length)
        {
            Faults |= (_count + length) > _capacity ? FaultCodes.CapacityExceeded : FaultCodes.None;
            return Faults == FaultCodes.None;
        }
    }
}
