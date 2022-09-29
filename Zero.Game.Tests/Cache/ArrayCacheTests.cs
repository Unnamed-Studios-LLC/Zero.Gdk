using NUnit.Framework;
using Zero.Game.Shared;

namespace Zero.Game.Tests
{
    public class ArrayCacheTests
    {
        [Test]
        public void Get()
        {
            var cache = new ArrayCache<byte>(10, 100, 2);

            var length = 20;
            var expectedSize = 100;

            var array = cache.Get(length);
            Assert.AreEqual(expectedSize, array.Length);
        }

        [Test]
        public void Return_TooSmall()
        {
            var cache = new ArrayCache<byte>(10, 100, 2);

            var array = new byte[20];
            cache.Return(array);

            var length = 20;
            var expectedSize = 100;

            array = cache.Get(length);
            Assert.AreEqual(expectedSize, array.Length);
        }

        [Test]
        public void Return_NonExact()
        {
            var cache = new ArrayCache<byte>(10, 100, 2);

            var array = new byte[120];
            cache.Return(array);

            var length = 20;
            var expectedSize = 120;

            array = cache.Get(length);
            Assert.AreEqual(expectedSize, array.Length);
        }
    }
}
