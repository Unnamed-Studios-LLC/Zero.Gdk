using System;
using System.Collections.Generic;

namespace Zero.Game.Shared
{
    internal unsafe class EntityData
    {
        private readonly ArrayCache<byte> _bufferCache;
        private readonly Dictionary<byte, int> _persistentLocationMap = new Dictionary<byte, int>();
        private readonly int _minSize;

        private int _eventDataCapacity;
        private long _eventTime;

        private int _persistentDataCapacity;

        public EntityData(int minSize, ArrayCache<byte> bufferCache)
        {
            _minSize = minSize;
            _bufferCache = bufferCache;

            _eventDataCapacity = minSize;
            _persistentDataCapacity = minSize;

            EventData = bufferCache.Get(_eventDataCapacity);
            PersistentData = bufferCache.Get(_persistentDataCapacity);
        }

        public byte[] EventData { get; private set; }
        public int EventDataByteLength { get; private set; }
        public int EventDataCount { get; private set; }

        public byte[] PersistentData { get; private set; }
        public int PersistentDataByteLength { get; private set; }
        public int PersistentDataCount { get; private set; }

        public void Clear()
        {
            _bufferCache.Return(EventData);
            EventData = _bufferCache.Get(_minSize);
            _eventDataCapacity = _minSize;
            EventDataCount = 0;
            EventDataByteLength = 0;
            _eventTime = 0;

            _bufferCache.Return(PersistentData);
            PersistentData = _bufferCache.Get(_minSize);
            _persistentDataCapacity = _minSize;
            PersistentDataCount = 0;
            PersistentDataByteLength = 0;

            _persistentLocationMap.Clear();
        }

        public T GetPersistent<T>() where T : unmanaged
        {
            var type = Data<T>.Type;
            var location = _persistentLocationMap[type];
            if (location == 0)
            {
                return default;
            }

            fixed (byte* buffer = &PersistentData[location])
            {
                return *(T*)buffer;
            }
        }

        public bool HasEventData(long time)
        {
            return time == _eventTime && EventDataCount != 0;
        }

        public void PushEvent<T>(long time, T* data) where T : unmanaged
        {
            if (time != _eventTime)
            {
                EventDataCount = 0;
                EventDataByteLength = 0;
                _eventTime = time;
            }

            var newSize = EventDataByteLength + 1 + sizeof(T);
            if (newSize > _eventDataCapacity)
            {
                // expand
                ExpandEventData(newSize);
            }

            EventDataCount++;
            fixed (byte* buffer = &EventData[EventDataByteLength])
            {
                *buffer = Data<T>.Type;
                *(T*)(buffer + 1) = *data;
            }
            EventDataByteLength = newSize;
        }

        public void PushPersistent<T>(T* data) where T : unmanaged
        {
            var type = Data<T>.Type;
            if (!_persistentLocationMap.TryGetValue(type, out var location))
            {
                // set location
                var newSize = PersistentDataByteLength + 1 + sizeof(T);
                if (newSize > _persistentDataCapacity)
                {
                    // expand
                    ExpandPersistentData(newSize);
                }

                PersistentDataCount++;
                PersistentData[PersistentDataByteLength] = type;
                _persistentLocationMap[type] = location = PersistentDataByteLength + 1;
                PersistentDataByteLength = newSize;
            }

            fixed (byte* buffer = &PersistentData[location])
            {
                *(T*)buffer = *data;
            }
        }

        private static byte[] Expand(ArrayCache<byte> bufferCache, byte[] current, int currentCount, int newSize)
        {
            var @new = bufferCache.Get(newSize);
            fixed (byte* newPointer = &@new[0])
            fixed (byte* currentPointer = &current[0])
            {
                Buffer.MemoryCopy(currentPointer, newPointer, newSize, currentCount);
            }
            bufferCache.Return(current);
            return @new;
        }

        private void ExpandEventData(int minSize)
        {
            do
            {
                _eventDataCapacity *= 2;
            }
            while (_eventDataCapacity < minSize);
            EventData = Expand(_bufferCache, EventData, EventDataByteLength, _eventDataCapacity);
        }

        private void ExpandPersistentData(int minSize)
        {
            do
            {
                _persistentDataCapacity *= 2;
            }
            while (_persistentDataCapacity < minSize);
            PersistentData = Expand(_bufferCache, PersistentData, PersistentDataByteLength, _persistentDataCapacity);
        }
    }
}