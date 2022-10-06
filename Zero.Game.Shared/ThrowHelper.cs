using System;
using System.Runtime.CompilerServices;

namespace Zero.Game.Shared
{
    internal class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowInvalidEntityId()
        {
            throw new Exception("Entity does not exist at the given id");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfDataNotDefined<T>() where T : unmanaged
        {
            if (!Data<T>.Generated)
            {
                throw new Exception($"Data {typeof(T).FullName} has not been defined in the DataBuilder");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ThrowWorldInParallel()
        {
            throw new Exception("Unable to enter ParallelForEach, the executing world is marked as Parallel update");
        }
    }
}
