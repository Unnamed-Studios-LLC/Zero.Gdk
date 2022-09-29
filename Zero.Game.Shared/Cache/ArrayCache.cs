using System.Collections.Generic;

namespace Zero.Game.Shared
{
    internal class ArrayCache<T>
    {
        private readonly int _bucketCount;
        private readonly int _minSize;
        private readonly int _scalar;
        private readonly Stack<T[]>[] _buckets;

        public ArrayCache(int bucketCount, int minSize, int scalar)
        {
            _bucketCount = bucketCount;
            _minSize = minSize;
            _scalar = scalar;
            _buckets = new Stack<T[]>[bucketCount];

            for (int i = 0; i < _bucketCount; i++)
            {
                _buckets[i] = new Stack<T[]>();
            }
        }

        public T[] Get(int minSize)
        {
            int bucket = GetBucketIndex(minSize, out var arraySize);
            if (bucket >= _bucketCount)
            {
                return new T[minSize];
            }

            var stack = _buckets[bucket];
            lock (stack)
            {
                if (stack.Count == 0)
                {
                    return new T[arraySize];
                }

                return stack.Pop();
            }
        }

        public void Return(T[] array)
        {
            int bucket = GetBucketIndex(array.Length, out _) - 1;
            if (bucket < 0 ||
                bucket >= _bucketCount)
            {
                return;
            }

            var stack = _buckets[bucket];
            lock (stack)
            {
                stack.Push(array);
            }
        }

        private int GetBucketIndex(int length, out int arraySize)
        {
            int bucket = 0;
            arraySize = _minSize;
            while (length > arraySize)
            {
                bucket++;
                arraySize *= _scalar;
            }
            return bucket;
        }
    }
}
