using System;
using System.Collections.Generic;

namespace Zero.Game.Server
{
    public static class ListExtensions
    {
        public static bool PatchRemove<T>(this List<T> list, T value)
        {
            var index = list.IndexOf(value);
            if (index == -1)
            {
                return false;
            }

            list.PatchRemoveAt(index);
            return true;
        }

        public static void PatchRemoveAt<T>(this List<T> list, int index)
        {
            if (index < 0 || index >= list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (index != list.Count - 1)
            {
                list[index] = list[^1];
            }

            list.RemoveAt(list.Count - 1);
        }
    }
}
