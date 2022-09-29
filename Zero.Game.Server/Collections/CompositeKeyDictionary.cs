using System.Collections.Generic;

namespace Zero.Game.Server
{
    public unsafe class CompositeKeyDictionary<TKey, TValue> where TKey : unmanaged
    {
        private class Locator
        {
            public bool HasValue;
            public TValue Value;
            public Dictionary<TKey, Locator> Map = new();
        }

        private readonly Locator _locators = new();

        public void Insert(TKey[] key, TValue value)
        {
            Insert(key, key.Length, value);
        }

        public void Insert(TKey[] key, int keyLength, TValue value)
        {
            fixed (TKey* keyPtr = key)
            {
                Insert(keyPtr, keyLength, value);
            }
        }

        public void Insert(TKey* key, int keyLength, TValue value)
        {
            int i = 0;
            Locator locator = _locators;
            while (i < keyLength)
            {
                var keyValue = key[i++];
                if (!locator.Map.TryGetValue(keyValue, out var newLocator))
                {
                    newLocator = new Locator();
                    locator.Map.Add(keyValue, newLocator);
                    locator = newLocator;
                }
                locator = newLocator;
            }
            locator.Value = value;
            locator.HasValue = true;
        }

        public bool TryGetValue(TKey[] key, out TValue value)
        {
            return TryGetValue(key, key.Length, out value);
        }

        public bool TryGetValue(TKey[] key, int keyLength, out TValue value)
        {
            fixed (TKey* keyPtr = key)
            {
                return TryGetValue(keyPtr, keyLength, out value);
            }
        }

        public bool TryGetValue(TKey* key, int keyLength, out TValue value)
        {
            int i = 0;
            Locator locator = _locators;
            while (locator.Map.TryGetValue(key[i++], out locator))
            {
                if (i == keyLength)
                {
                    value = locator.Value;
                    return locator.HasValue;
                }
            }

            value = default;
            return false;
        }
    }
}
