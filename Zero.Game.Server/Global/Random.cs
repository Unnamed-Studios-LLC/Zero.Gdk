using System;
using System.Linq;
using System.Text;
using System.Threading;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public static class Random
    {
        private readonly static ThreadLocal<InternalRandom> s_randomPool = new(() => new InternalRandom());

        private static InternalRandom LocalRandom => s_randomPool.Value;

        public static float Degree() => LocalRandom.Degree();

        public static float Float01() => LocalRandom.Float01();
        public static float FloatRange(float min, float max) => LocalRandom.FloatRange(min, max);

        public static int Int() => LocalRandom.Int();
        public static int IntRange(int min, int max) => LocalRandom.IntRange(min, max);

        public static float Radian() => LocalRandom.Radian();

        public static string String(int length, char[] characterPool) => LocalRandom.String(length, characterPool);
        public static string StringAlphabet(int length) => LocalRandom.StringAlphabet(length);
        public static string StringAlphaNumeric(int length) => LocalRandom.StringAlphaNumeric(length);
        public static string StringNumeric(int length) => LocalRandom.StringNumeric(length);

        private class InternalRandom
        {
            private readonly static char[] s_alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            private readonly static char[] s_numeric = "0123456789".ToCharArray();
            private readonly static char[] s_alphaNumeric = s_alphabet.Concat(s_numeric).ToArray();

            private readonly System.Random _random = new((int)DateTime.Now.Ticks);

            public float Degree()
            {
                return Radian() * Angle.Rad2Deg;
            }

            public float Float01()
            {
                return (float)_random.NextDouble();
            }

            public float FloatRange(float min, float max)
            {
                return min + (max - min) * Float01();
            }

            public int Int()
            {
                return _random.Next();
            }

            public int IntRange(int min, int max)
            {
                return _random.Next(min, max);
            }

            public float Radian()
            {
                return Float01() * (float)Math.PI * 2f;
            }

            public string String(int length, char[] characterPool)
            {
                var builder = new StringBuilder(length);
                for (int i = 0; i < length; i++)
                {
                    builder.Append(characterPool[IntRange(0, characterPool.Length)]);
                }
                return builder.ToString();
            }

            public string StringAlphabet(int length)
            {
                return String(length, s_alphabet);
            }

            public string StringAlphaNumeric(int length)
            {
                return String(length, s_alphaNumeric);
            }

            public string StringNumeric(int length)
            {
                return String(length, s_numeric);
            }
        }
    }
}
