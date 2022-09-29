using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zero.Game.Server
{
    internal static class ArrayHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TType AsRef<TType>(this TType[] array)
        {
            ref var arrayRef = ref MemoryMarshal.GetArrayDataReference(array);
            return ref arrayRef;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TType GetFromIndex<TType>(this TType[] array, uint index)
        {
            ref var arrayRef = ref MemoryMarshal.GetArrayDataReference(array);
            return ref Unsafe.Add(ref arrayRef, unchecked((int)index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TType GetFromIndex<TType>(this TType[] array, int index)
        {
            ref var arrayRef = ref MemoryMarshal.GetArrayDataReference(array);
            return ref Unsafe.Add(ref arrayRef, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TType GetFromIndex<TType>(ref TType arrayRef, uint index)
        {
            return ref Unsafe.Add(ref arrayRef, unchecked((int)index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TType GetFromOffsetIndex<TType>(this TType[] array, uint index)
        {
            ref var arrayRef = ref MemoryMarshal.GetArrayDataReference(array);
            return ref Unsafe.Add(ref arrayRef, unchecked((int)index) - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TType GetFromOffsetIndex<TType>(ref TType arrayRef, uint index)
        {
            return ref Unsafe.Add(ref arrayRef, unchecked((int)index) - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TType Next<TType>(ref TType elementRef)
        {
            return ref Unsafe.Add(ref elementRef, 1);
        }
    }
}
