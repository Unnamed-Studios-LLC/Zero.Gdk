using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zero.Game.Server
{
    public class ValueList<T> where T : struct
    {
        private readonly uint _elementSize = (uint)Marshal.SizeOf<T>();
        private T[] _array;
        private uint _count;
        private uint _capacity;
        private T _empty;

        public ValueList(uint initialCapacity)
        {
            _capacity = initialCapacity;
            _array = new T[_capacity];
        }

        public T[] Array => _array;
        public ref T this[uint index] => ref ((index >= _count) ? ref _empty : ref _array[index]);
        public ref T this[int index] => ref ((index >= _count) ? ref _empty : ref _array[index]);

        public uint Count => _count;

        public void Add(T value)
        {
            if (_count >= _capacity)
            {
                var newCapacity = _capacity * 2;
                var newArray = new T[newCapacity];

                ref var arrayRef = ref MemoryMarshal.GetReference(MemoryMarshal.AsBytes(_array.AsSpan()));
                ref var newArrayRef = ref MemoryMarshal.GetReference(MemoryMarshal.AsBytes(newArray.AsSpan()));
                Unsafe.CopyBlock(ref newArrayRef, ref arrayRef, _elementSize * _capacity);

                _array = newArray;
                _capacity = newCapacity;
            }

            ref var elementRef = ref ArrayHelper.GetFromIndex(ref _array.AsRef(), _count++);
            elementRef = value;
        }

        public void Clear()
        {
            _count = 0;
        }
    }
}
