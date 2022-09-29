using System;
using System.Runtime.CompilerServices;

namespace Zero.Game.Server
{
    internal unsafe static class ComponentList
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(byte* source, int sourceIndex, byte* destination, int destinationIndex, int elementSize)
        {
            Buffer.MemoryCopy(source + sourceIndex * elementSize, destination + destinationIndex * elementSize, elementSize, elementSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(byte* pointer, int sourceIndex, int destinationIndex) where T : unmanaged
        {
            *((T*)pointer + destinationIndex) = *((T*)pointer + sourceIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T>(byte* source, int index) where T : unmanaged
        {
            return ref Unsafe.AsRef<T>(source + index * sizeof(T));
        }
    }
}
