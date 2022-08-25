using System;
using System.Collections.Generic;
using System.Text;
using Zero.Game.Shared;

namespace Zero.Game.Common
{
    public class BitReader : ISReader
    {
        /// <summary>
        /// Scratch value used to read values
        /// </summary>
        private ulong _scratch = 0;

        /// <summary>
        /// Length of bits left in the scratch value
        /// </summary>
        private int _scratchBits = 0;

        /// <summary>
        /// Bit data buffer
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        /// The amount of bits read from the bit data buffer
        /// </summary>
        private long _bytesRead = 0;

        private int _offset;

        public BitReader(byte[] data)
        {
            Length = data.Length;
            InitBuffer(data, 0, data.Length);
        }

        public BitReader(byte[] data, int offset, int length)
        {
            Length = length;
            InitBuffer(data, offset, length);
        }

        public int Length { get; }

        private void InitBuffer(byte[] data, int offset, int length)
        {
            _buffer = data;
            _offset = offset;
        }

        /// <summary>
        /// Gets data from the buffer and adds 32 bits to the scratch
        /// </summary>
        private void FillScratch()
        {
            var index = _offset + _bytesRead;
            unsafe
            {
                fixed (byte* addr = &_buffer[index])
                {
                    var intAddr = (uint*)addr;
                    ulong bits = *intAddr;
                    _bytesRead += 4;

                    _scratch |= bits << _scratchBits;
                    _scratchBits += 32;
                }
            }
            /*
            ulong bits = (_buffer[index++]) |
                ((ulong)_buffer[index++] << 8) |
                ((ulong)_buffer[index++] << 16) |
                ((ulong)_buffer[index++] << 24);
            */
        }

        private T[] ReadArray<T>(Func<T> reader)
        {
            var length = ReadArrayLength();

            var array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = reader();
            }
            return array;
        }

        private List<T> ReadList<T>(List<T> list, Func<T> reader)
        {
            list.Clear();
            var length = ReadArrayLength();

            for (int i = 0; i < length; i++)
            {
                list.Add(reader());
            }
            return list;
        }

        public uint ReadArrayLength()
        {
            var length = (uint)ReadUInt8();
            var isLarge = ((length >> 7) & 1) == 1;

            if (isLarge)
            {
                length &= 0x7f;
                var additionalLengthByte = (uint)ReadUInt8();
                length |= additionalLengthByte << 7;
            }

            return length;
        }

        /// <summary>
        /// Reads bits from the buffer
        /// </summary>
        /// <param name="length">The amount of bits to read (max 32)</param>
        /// <returns>The bits read</returns>
        public uint Read(byte length)
        {
            if (_scratchBits < 32)
            {
                FillScratch();
            }

            int shift = 32 + 32 - length;
            uint bits = (uint)((_scratch << shift) >> shift);

            _scratch >>= length;
            _scratchBits -= length;

            return bits;
        }

        public bool ReadBool() => Read(1) == 1;
        public bool[] ReadBoolArray() => ReadArray(ReadBool);

        public byte ReadUInt8() => (byte)Read(8);
        public byte[] ReadUInt8Array() => ReadArray(ReadUInt8);
        public ushort ReadUInt16() => (ushort)Read(16);
        public ushort[] ReadUInt16Array() => ReadArray(ReadUInt16);
        public uint ReadUInt32() => Read(32);
        public uint[] ReadUInt32Array() => ReadArray(ReadUInt32);
        public ulong ReadUInt64()
        {
            ulong a = Read(32);
            ulong b = ((ulong)Read(32) << 32);
            return a | b;
        }
        public ulong[] ReadUInt64Array() => ReadArray(ReadUInt64);

        public sbyte ReadInt8() => (sbyte)Read(8);
        public sbyte[] ReadInt8Array() => ReadArray(ReadInt8);
        public short ReadInt16() => (short)Read(16);
        public short[] ReadInt16Array() => ReadArray(ReadInt16);
        public int ReadInt32() => (int)Read(32);
        public int[] ReadInt32Array() => ReadArray(ReadInt32);
        public long ReadInt64() => (long)Read(32) | ((long)Read(32) << 32);
        public long[] ReadInt64Array() => ReadArray(ReadInt64);

        public float ReadFloat()
        {
            uint intVal = Read(32);
            unsafe
            {
                return *((float*)&intVal);
            }
        }
        public float[] ReadFloatArray() => ReadArray(ReadFloat);

        public double ReadDouble()
        {
            ulong intVal = ReadUInt64();
            unsafe
            {
                return *((double*)&intVal);
            }
        }
        public double[] ReadDoubleArray() => ReadArray(ReadDouble);

        public string ReadUtf()
        {
            var length = ReadUInt16();
            if (length == 0)
            {
                return string.Empty;
            }

            byte[] bytes = ReadBytes(length);
            return Encoding.UTF8.GetString(bytes, 0, length);
        }
        public string[] ReadUtfArray() => ReadArray(ReadUtf);

        public byte[] ReadBytes(int length)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
                bytes[i] = ReadUInt8();
            return bytes;
        }

        public uint Value(uint input, byte bitLength)
        {
            return Read(bitLength);
        }

        public bool Value(bool input)
        {
            return ReadBool();
        }

        public bool[] Value(bool[] input)
        {
            return ReadBoolArray();
        }

        public byte Value(byte input)
        {
            return ReadUInt8();
        }

        public byte[] Value(byte[] input)
        {
            return ReadUInt8Array();
        }

        public ushort Value(ushort input)
        {
            return ReadUInt16();
        }

        public ushort[] Value(ushort[] input)
        {
            return ReadUInt16Array();
        }

        public uint Value(uint input)
        {
            return ReadUInt32();
        }

        public uint[] Value(uint[] input)
        {
            return ReadUInt32Array();
        }

        public ulong Value(ulong input)
        {
            return ReadUInt64();
        }

        public ulong[] Value(ulong[] input)
        {
            return ReadUInt64Array();
        }

        public sbyte Value(sbyte input)
        {
            return ReadInt8();
        }

        public sbyte[] Value(sbyte[] input)
        {
            return ReadInt8Array();
        }

        public short Value(short input)
        {
            return ReadInt16();
        }

        public short[] Value(short[] input)
        {
            return ReadInt16Array();
        }

        public int Value(int input)
        {
            return ReadInt32();
        }

        public int[] Value(int[] input)
        {
            return ReadInt32Array();
        }

        public long Value(long input)
        {
            return ReadInt64();
        }

        public long[] Value(long[] input)
        {
            return ReadInt64Array();
        }

        public float Value(float input)
        {
            return ReadFloat();
        }

        public float[] Value(float[] input)
        {
            return ReadFloatArray();
        }

        public double Value(double input)
        {
            return ReadDouble();
        }

        public double[] Value(double[] input)
        {
            return ReadDoubleArray();
        }

        public string Value(string input)
        {
            return ReadUtf();
        }

        public string[] Value(string[] input)
        {
            return ReadUtfArray();
        }

        public byte[] Value(byte[] input, int byteLength)
        {
            return ReadBytes(byteLength);
        }

        public T Value<T>(T input) where T : ISerializable, new()
        {
            var value = new T();
            value.Serialize(this);
            return value;
        }

        public T[] Value<T>(T[] input) where T : ISerializable, new()
        {
            T readFunc()
            {
                var v = new T();
                v.Serialize(this);
                return v;
            }
            return ReadArray(readFunc);
        }

        public List<T> Value<T>(List<T> input) where T : ISerializable, new()
        {
            T readFunc()
            {
                var v = new T();
                v.Serialize(this);
                return v;
            }
            return ReadList(input, readFunc);
        }

        public List<bool> Value(List<bool> input)
        {
            return ReadList(input, ReadBool);
        }

        public List<byte> Value(List<byte> input)
        {
            return ReadList(input, ReadUInt8);
        }

        public List<ushort> Value(List<ushort> input)
        {
            return ReadList(input, ReadUInt16);
        }

        public List<uint> Value(List<uint> input)
        {
            return ReadList(input, ReadUInt32);
        }

        public List<ulong> Value(List<ulong> input)
        {
            return ReadList(input, ReadUInt64);
        }

        public List<sbyte> Value(List<sbyte> input)
        {
            return ReadList(input, ReadInt8);
        }

        public List<short> Value(List<short> input)
        {
            return ReadList(input, ReadInt16);
        }

        public List<int> Value(List<int> input)
        {
            return ReadList(input, ReadInt32);
        }

        public List<long> Value(List<long> input)
        {
            return ReadList(input, ReadInt64);
        }

        public List<float> Value(List<float> input)
        {
            return ReadList(input, ReadFloat);
        }

        public List<double> Value(List<double> input)
        {
            return ReadList(input, ReadDouble);
        }

        public List<string> Value(List<string> input)
        {
            return ReadList(input, ReadUtf);
        }
    }
}
