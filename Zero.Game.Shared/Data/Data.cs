using System;
using System.Linq;
using System.Reflection;

namespace Zero.Game.Shared
{
    internal static class Data
    {
        public const int MaxTypes = 255;

        public static object Lock = new object();
        public static int NextType = 0;
    }

    internal static class Data<T> where T : unmanaged
    {
        public static T NullRef = default;

        public static bool Generated { get; private set; }
        public static byte Type { get; private set; }
        public static bool ZeroSize { get; private set; }
        public static int Size { get; private set; }

        internal unsafe static void Generate()
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
                ZeroSize = GenerateIsZeroSize(typeof(T));
                Size = ZeroSize ? 0 : sizeof(T);
            }
        }

        private static bool GenerateIsZeroSize(Type type)
        {
            var zeroSize = type.IsValueType && !type.IsPrimitive &&
                type.GetFields((BindingFlags)0x34).All(fi => GenerateIsZeroSize(fi.FieldType));
            return zeroSize;
        }
    }
}
