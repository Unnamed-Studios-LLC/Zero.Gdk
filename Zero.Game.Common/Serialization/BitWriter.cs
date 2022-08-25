using System;
using System.Collections.Generic;
using System.Text;
using Zero.Game.Shared;

namespace Zero.Game.Common
{
    public class BitWriter : ISWriter
    {
        /// <summary>
        /// The default buffer length of BitWriter
        /// </summary>
        //private const int Default_Length = 1024;

        /// <summary>
        /// Scratch bits used to hold currently written bits
        /// </summary>
        private ulong _scratch = 0;

        /// <summary>
        /// The amount of bits still in the scratch value
        /// </summary>
        private int _scratchBits = 0;

        /// <summary>
        /// The amount of bits written into the buffer
        /// </summary>
        private uint _bitsWritten = 0;

        /// <summary>
        /// Buffer of already written bits
        /// </summary>
        private readonly byte[] _buffer;

        public long Length => (_bitsWritten + _scratchBits + 7) / 8;

        public BitWriter(byte[] buffer)
        {
            _buffer = buffer;
        }

        /*
        /// <summary>
        /// Expands the buffer by the default length
        /// </summary>
        private void ExpandBuffer()
        {
            byte[] newBuffer = new byte[buffer.Length + Default_Length];
            System.Buffer.BlockCopy(buffer, 0, newBuffer, 0, sizeof(uint) * buffer.Length);
            buffer = newBuffer;
        }
        */

        /// <summary>
        /// Adds bits to the bit buffer
        /// </summary>
        /// <param name="bits"></param>
        private unsafe void AddToBuffer(uint bits)
        {
            uint index = _bitsWritten / 8;
            if (index + 3 >= _buffer.Length)
            {
                throw new IndexOutOfRangeException();
            }

            fixed (byte* addr = &_buffer[index])
            {
                var intAddr = (uint*)addr;
                *intAddr = bits;
            }
        }

        /// <summary>
        /// Flushes the last bits from the scratch. Only flushes a maximum of 32 bits
        /// </summary>
        /// <returns></returns>
        private void FlushScratch()
        {
            AddToBuffer((uint)_scratch);
            _scratch >>= 32;
            _scratchBits -= 32;
            _bitsWritten += 32;
        }

        private void WriteArray<T>(T[] array, Action<T> writer)
        {
            var length = (uint)(array?.Length ?? 0);
            WriteArrayLength(length);
            for (int i = 0; i < length; i++)
            {
                writer(array[i]);
            }
        }

        private void WriteList<T>(List<T> list, Action<T> writer)
        {
            var length = (uint)(list?.Count ?? 0);
            WriteArrayLength(length);
            for (int i = 0; i < length; i++)
            {
                writer(list[i]);
            }
        }

        public void WriteArrayLength(uint length)
        {
            if (length > 127)
            {
                var firstByte = (byte)(length | 0x80);
                Write(firstByte);
                Write((byte)(length >> 7));
            }
            else
            {
                Write((byte)length);
            }
        }

        /// <summary>
        /// Clears the buffer and resets writing
        /// </summary>
        public void Clear()
        {
            _scratch = 0;
            _scratchBits = 0;
            _bitsWritten = 0;
        }

        /// <summary>
        /// Returns a buffer containing all writen data.
        /// </summary>
        /// <returns></returns>
        public ByteBuffer GetBuffer()
        {
            int index = (int)(_bitsWritten / 8);
            if (_scratchBits > 0)
            {
                int scratchBytes = (_scratchBits + 7) / 8;
                for (int i = 0; i < scratchBytes; i++)
                {
                    byte b = (byte)(_scratch >> (i * 8));
                    _buffer[index++] = b;
                }
            }
            return new ByteBuffer(_buffer, index);
        }
        
        /// <summary>
        /// Writes bits to the buffer
        /// </summary>
        /// <param name="bits">The uint32 containing the bits</param>
        /// <param name="length">The amount of bits to write</param>
        public void Write(uint bits, byte length)
        {
            int cleanShift = 32 - length;
            ulong cleanBits = (bits << cleanShift) >> cleanShift;

            _scratch |= (cleanBits << _scratchBits);
            _scratchBits += length;

            if (_scratchBits < 32) return;
            FlushScratch();
        }

        public void Write(bool value) => Write(value ? (uint)1 : 0, 1);
        public void Write(bool[] value) => WriteArray(value, Write);

        public void Write(byte value) => Write(value, 8);
        public void Write(byte[] value) => WriteArray(value, Write);
        public void Write(ushort value) => Write(value, 16);
        public void Write(ushort[] value) => WriteArray(value, Write);
        public void Write(uint value) => Write(value, 32);
        public void Write(uint[] value) => WriteArray(value, Write);
        public void Write(ulong value)
        {
            Write((uint)value, 32);
            Write((uint)(value >> 32), 32);
        }
        public void Write(ulong[] value) => WriteArray(value, Write);

        public void Write(sbyte value) => Write((uint)value, 8);
        public void Write(sbyte[] value) => WriteArray(value, Write);
        public void Write(short value) => Write((uint)value, 16);
        public void Write(short[] value) => WriteArray(value, Write);
        public void Write(int value) => Write((uint)value, 32);
        public void Write(int[] value) => WriteArray(value, Write);
        public void Write(long value)
        {
            Write((uint)value, 32);
            Write((uint)(value >> 32), 32);
        }
        public void Write(long[] value) => WriteArray(value, Write);

        public void Write(float value)
        {
            uint intVal = 0;
            unsafe
            {
                intVal = *((uint*)&value);
            }
            Write(intVal, 32);
        }
        public void Write(float[] value) => WriteArray(value, Write);

        public void Write(double value)
        {
            ulong intVal = 0;
            unsafe
            {
                intVal = *((ulong*)&value);
            }
            Write(intVal);
        }
        public void Write(double[] value) => WriteArray(value, Write);

        public void Write(string str)
        {
            if (str == null)
            {
                Write((ushort)0);
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(str);
            Write((ushort)bytes.Length);
            WriteBytes(bytes);
        }
        public void Write(string[] value) => WriteArray(value, Write);

        public void WriteBytes(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
                Write(bytes[i]);
        }

        public uint Value(uint input, byte bitLength)
        {
            Write(input, bitLength);
            return input;
        }

        public bool Value(bool input)
        {
            Write(input);
            return input;
        }

        public bool[] Value(bool[] input)
        {
            Write(input);
            return input;
        }

        public byte Value(byte input)
        {
            Write(input);
            return input;
        }

        public byte[] Value(byte[] input)
        {
            Write(input);
            return input;
        }

        public ushort Value(ushort input)
        {
            Write(input);
            return input;
        }

        public ushort[] Value(ushort[] input)
        {
            Write(input);
            return input;
        }

        public uint Value(uint input)
        {
            Write(input);
            return input;
        }

        public uint[] Value(uint[] input)
        {
            Write(input);
            return input;
        }

        public ulong Value(ulong input)
        {
            Write(input);
            return input;
        }

        public ulong[] Value(ulong[] input)
        {
            Write(input);
            return input;
        }

        public sbyte Value(sbyte input)
        {
            Write(input);
            return input;
        }

        public sbyte[] Value(sbyte[] input)
        {
            Write(input);
            return input;
        }

        public short Value(short input)
        {
            Write(input);
            return input;
        }

        public short[] Value(short[] input)
        {
            Write(input);
            return input;
        }

        public int Value(int input)
        {
            Write(input);
            return input;
        }

        public int[] Value(int[] input)
        {
            Write(input);
            return input;
        }

        public long Value(long input)
        {
            Write(input);
            return input;
        }

        public long[] Value(long[] input)
        {
            Write(input);
            return input;
        }

        public float Value(float input)
        {
            Write(input);
            return input;
        }

        public float[] Value(float[] input)
        {
            Write(input);
            return input;
        }

        public double Value(double input)
        {
            Write(input);
            return input;
        }

        public double[] Value(double[] input)
        {
            Write(input);
            return input;
        }

        public string Value(string input)
        {
            Write(input);
            return input;
        }

        public string[] Value(string[] input)
        {
            Write(input);
            return input;
        }

        public byte[] Value(byte[] input, int byteLength)
        {
            WriteBytes(input);
            return input;
        }

        public T Value<T>(T input) where T : ISerializable, new()
        {
            input.Serialize(this);
            return input;
        }

        public T[] Value<T>(T[] input) where T : ISerializable, new()
        {
            void writeFunc(T v) => v.Serialize(this);
            WriteArray(input, writeFunc);
            return input;
        }

        public List<T> Value<T>(List<T> input) where T : ISerializable, new()
        {
            void writeFunc(T v) => v.Serialize(this);
            WriteList(input, writeFunc);
            return input;
        }

        public List<bool> Value(List<bool> input)
        {
            WriteList(input, Write);
            return input;
        }

        public List<byte> Value(List<byte> input)
        {
            WriteList(input, Write);
            return input;
        }

        public List<ushort> Value(List<ushort> input)
        {
            WriteList(input, Write);
            return input;
        }

        public List<uint> Value(List<uint> input)
        {
            WriteList(input, Write);
            return input;
        }

        public List<ulong> Value(List<ulong> input)
        {
            WriteList(input, Write);
            return input;
        }

        public List<sbyte> Value(List<sbyte> input)
        {
            WriteList(input, Write);
            return input;
        }

        public List<short> Value(List<short> input)
        {
            WriteList(input, Write);
            return input;
        }

        public List<int> Value(List<int> input)
        {
            WriteList(input, Write);
            return input;
        }

        public List<long> Value(List<long> input)
        {
            WriteList(input, Write);
            return input;
        }

        public List<float> Value(List<float> input)
        {
            WriteList(input, Write);
            return input;
        }

        public List<double> Value(List<double> input)
        {
            WriteList(input, Write);
            return input;
        }

        public List<string> Value(List<string> input)
        {
            WriteList(input, Write);
            return input;
        }
    }
}
