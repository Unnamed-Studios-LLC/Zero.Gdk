using System;

namespace Zero.Game.Shared
{
    internal static class Data
    {
        public const int MaxTypes = 256;

        public static object Lock = new object();
        public static int NextType = 0;
    }

    internal static class Data<T> where T : unmanaged
    {
        public static T NullRef = default;

        public static bool Generated { get; private set; }
        public static byte Type { get; private set; }

        internal static void Generate()
        {
            lock (Data.Lock)
            {
                if (Generated)
                {
                    return;
                }

                if (Data.NextType == Data.MaxTypes)
                {
                    throw new Exception("Maximum types reached");
                }

                Type = (byte)Data.NextType++;
                Generated = true;
            }
        }
    }
}
