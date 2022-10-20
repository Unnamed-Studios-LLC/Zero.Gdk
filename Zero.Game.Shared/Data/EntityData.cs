using System;
using System.Collections.Generic;
using System.Linq;

namespace Zero.Game.Shared
{
    internal unsafe class EntityData
    {
        public struct DataBuffer
        {
            public byte[] Buffer;
            public int Capacity;
            public int ByteLength;
            public int Count;

            public DataBuffer(byte[] buffer, int capacity) : this()
            {
                Buffer = buffer;
                Capacity = capacity;
            }

            public void Clear(ArrayCache<byte> bufferCache, int capacity)
            {
                bufferCache.Return(Buffer);
                Buffer = bufferCache.Get(capacity);
                Capacity = capacity;
                ByteLength = 0;
                Count = 0;
            }

            public void Reset()
            {
                ByteLength = 0;
                Count = 0;
            }

            public bool Write(ref BlitWriter writer)
            {
                if (Count != 0)
                {
                    fixed (byte* dataBufferPtr = Buffer)
                    {
                        if (!writer.Write(dataBufferPtr, ByteLength))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public DataBuffer EventData;
        public DataBuffer PersistentData;
        public DataBuffer PersistentUpdatedData;

        private readonly ArrayCache<byte> _bufferCache;
        private readonly Dictionary<byte, int> _persistentLocationMap = new Dictionary<byte, int>();
        private readonly Dictionary<byte, int> _persistentUpdatedLocationMap = new Dictionary<byte, int>();
        private readonly int _minSize;

        private long _oneOffTime;
        private byte _lastEventType;
        private ushort _currentEventScalarCount;

        public EntityData(int minSize, ArrayCache<byte> bufferCache)
        {
            _minSize = minSize;
            _bufferCache = bufferCache;

            EventData = new DataBuffer(bufferCache.Get(minSize), minSize);
            PersistentData = new DataBuffer(bufferCache.Get(minSize), minSize);
            PersistentUpdatedData = new DataBuffer(bufferCache.Get(minSize), minSize);
        }

        public void Clear()
        {
            _oneOffTime = 0;
            _lastEventType = byte.MaxValue;
            _currentEventScalarCount = 0;

            EventData.Clear(_bufferCache, _minSize);
            PersistentData.Clear(_bufferCache, _minSize);
            PersistentUpdatedData.Clear(_bufferCache, _minSize);

            _persistentLocationMap.Clear();
            _persistentUpdatedLocationMap.Clear();
        }

        public T GetPersistent<T>() where T : unmanaged
        {
            var type = Data<T>.Type;
            var location = _persistentLocationMap[type];
            if (location == 0 ||
                Data<T>.ZeroSize)
            {
                return default;
            }

            fixed (byte* buffer = &PersistentData.Buffer[location])
            {
                return *(T*)buffer;
            }
        }

        public bool HasEventData(long time)
        {
            return time == _oneOffTime && EventData.Count != 0;
        }

        public bool HasOneOffData(long time)
        {
            return time == _oneOffTime && (EventData.Count != 0 || PersistentUpdatedData.Count != 0);
        }

        public void PushEvent<T>(long time, T* data) where T : unmanaged
        {
            ClearOneOff(time);

            var type = Data<T>.Type;
            var dataSize = Data<T>.Size;
            var newSize = EventData.ByteLength;
            var isScalar = _lastEventType == type;
            var isStartingScalar = _currentEventScalarCount == 0;

            if (isScalar)
            {
                if (isStartingScalar)
                {
                    newSize += sizeof(byte) + sizeof(ushort); // add space for count and scalar indicator
                }
                newSize += dataSize; // add data size
            }
            else
            {
                newSize += sizeof(byte) + dataSize; // add space for type and data size
            }

            if (newSize > EventData.Capacity)
            {
                // expand
                ExpandEventData(newSize);
            }

            if (isScalar)
            {
                if (isStartingScalar)
                {
                    // shift old data up to allow space for scalar size and indicator
                    _currentEventScalarCount = 2;
                    var oldSize = sizeof(byte) + dataSize; // type and data size
                    fixed (byte* buffer = &EventData.Buffer[EventData.ByteLength - oldSize])
                    {
                        Buffer.MemoryCopy(buffer, buffer + sizeof(byte) + sizeof(ushort), oldSize, oldSize);
                        *buffer = byte.MaxValue; // write indicator
                        *(ushort*)(buffer + 1) = _currentEventScalarCount; // write count
                    }
                    EventData.ByteLength += sizeof(ushort) + sizeof(byte);
                }
                else
                {
                    var sizeOffset = _currentEventScalarCount * dataSize + sizeof(byte) + sizeof(ushort); // offset behind data's, type, and count
                    fixed (byte* buffer = &EventData.Buffer[EventData.ByteLength - sizeOffset])
                    {
                        *(ushort*)buffer = ++_currentEventScalarCount; // write new count
                    }

                    if (_currentEventScalarCount == ushort.MaxValue) // if count is max, stop scalar
                    {
                        _lastEventType = byte.MaxValue;
                        _currentEventScalarCount = 0;
                    }
                }

                if (dataSize != 0)
                {
                    fixed (byte* buffer = &EventData.Buffer[EventData.ByteLength])
                    {
                        *(T*)buffer = *data;
                    }
                }
            }
            else
            {
                _currentEventScalarCount = 0;
                EventData.Count++;
                fixed (byte* buffer = &EventData.Buffer[EventData.ByteLength])
                {
                    *buffer = type;
                    if (dataSize != 0)
                    {
                        *(T*)(buffer + 1) = *data;
                    }
                }
                _lastEventType = type;
            }

            EventData.ByteLength = newSize;
        }

        public void PushPersistent<T>(long time, T* data) where T : unmanaged
        {
            PushPersistentUpdate(time, data);

            var type = Data<T>.Type;
            var dataSize = Data<T>.Size;

            if (!_persistentLocationMap.TryGetValue(type, out var location))
            {
                // set location
                var newSize = PersistentData.ByteLength + 1 + dataSize;
                if (newSize > PersistentData.Capacity)
                {
                    // expand
                    ExpandPersistentData(newSize);
                }

                PersistentData.Count++;
                PersistentData.Buffer[PersistentData.ByteLength] = type;
                _persistentLocationMap[type] = location = PersistentData.ByteLength + 1;
                PersistentData.ByteLength = newSize;
            }

            if (dataSize != 0)
            {
                fixed (byte* buffer = &PersistentData.Buffer[location])
                {
                    *(T*)buffer = *data;
                }
            }
        }

        /*
        public void RemovePersistent<T>() where T : unmanaged
        {
            var type = Data<T>.Type;
            if (!_persistentLocationMap.TryGetValue(type, out var location)) return;
            var dataSize = Data<T>.Size;
            var removeSize = dataSize + 1; // 1 for the type byte

            // shift buffer
            fixed (byte* ptr = &PersistentData.Buffer[location - 1])
            {
                Buffer.MemoryCopy(ptr + dataSize, ptr - 1, removeSize, removeSize);
            }

            var buffer = _bufferCache.Get(_persistentLocationMap.Count);
            _persistentLocationMap.Keys.CopyTo(buffer, 0);
            for (int i = 0; i < _persistentLocationMap.Count; i++)
            {

            }
        }
        */

        public bool TryGetPersistent<T>(out T data) where T : unmanaged
        {
            var type = Data<T>.Type;
            var location = _persistentLocationMap[type];
            if (location == 0)
            {
                data = default;
                return false;
            }

            if (Data<T>.ZeroSize)
            {
                data = default;
                return true;
            }

            fixed (byte* buffer = &PersistentData.Buffer[location])
            {
                data = *(T*)buffer;
                return true;
            }
        }

        private void ClearOneOff(long time)
        {
            if (time == _oneOffTime)
            {
                return;
            }

            _oneOffTime = time;
            _lastEventType = byte.MaxValue;
            _currentEventScalarCount = 0;

            EventData.Reset();
            PersistentUpdatedData.Reset();

            _persistentUpdatedLocationMap.Clear();
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

        private void Expand(ref DataBuffer dataBuffer, int minSize)
        {
            do
            {
                dataBuffer.Capacity *= 2;
            }
            while (dataBuffer.Capacity < minSize);
            dataBuffer.Buffer = Expand(_bufferCache, dataBuffer.Buffer, dataBuffer.ByteLength, dataBuffer.Capacity);
        }

        private void ExpandEventData(int minSize) => Expand(ref EventData, minSize);
        private void ExpandPersistentData(int minSize) => Expand(ref PersistentData, minSize);
        private void ExpandPersistentUpdatedData(int minSize) => Expand(ref PersistentUpdatedData, minSize);

        private void PushPersistentUpdate<T>(long time, T* data) where T : unmanaged
        {
            ClearOneOff(time);

            var type = Data<T>.Type;
            var dataSize = Data<T>.Size;

            if (_persistentUpdatedLocationMap.TryGetValue(type, out var location))
            {
                if (dataSize != 0)
                {
                    fixed (byte* buffer = &PersistentUpdatedData.Buffer[location])
                    {
                        *(T*)buffer = *data;
                    }
                }
                return;
            }

            var newSize = PersistentUpdatedData.ByteLength + sizeof(byte) + dataSize;
            if (newSize > PersistentUpdatedData.Capacity)
            {
                // expand
                ExpandPersistentUpdatedData(newSize);
            }
            
            PersistentUpdatedData.Count++;
            fixed (byte* buffer = &PersistentUpdatedData.Buffer[PersistentUpdatedData.ByteLength])
            {
                *buffer = type;
                if (dataSize != 0)
                {
                    *(T*)(buffer + 1) = *data;
                }
            }

            PersistentUpdatedData.ByteLength = newSize;
            _persistentUpdatedLocationMap[type] = PersistentUpdatedData.ByteLength - dataSize;
        }
    }
}