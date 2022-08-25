using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Text;
using Zero.Game.Shared;

namespace Zero.Game.Tests.Performance
{
    public class PerformanceTests
    {
        private enum TestType
        {
            A,
            B
        }

        private abstract class TestClass
        {
            public abstract ushort Type { get; }
        }

        private class TestAClass : TestClass
        {
            public override ushort Type => (ushort)TestType.A;
        }

        private class TestBClass : TestClass
        {
            public override ushort Type => (ushort)TestType.B;
        }

        [TestCase(90, -90)]
        [TestCase(90, -45)]
        [TestCase(90, 0)]
        [TestCase(90, 45)]
        [TestCase(90, 135)]
        [TestCase(90, 180)]
        [TestCase(90, 225)]
        [Explicit]
        public void Sandbox(float a, float b)
        {
            a *= Angle.Deg2Rad;
            b *= Angle.Deg2Rad;

            var vecA = Angle.Vec2(a);
            var vecB = Angle.Vec2(b);

            var magnitude = (vecA + vecB).Magnitude;
            var angleDif = Angle.MinDifference(a, b);

            Console.WriteLine("M: " + (magnitude - 1f));
            Console.WriteLine("A: " + angleDif * Angle.Rad2Deg);
        }

        [Test]
        [Explicit]
        public void TestMethod()
        {
            var stopwatch = new Stopwatch();
            int loops = 1_000_000;

            var classes = new TestClass[]
            {
                new TestAClass(),
                new TestAClass(),
                new TestAClass(),
                new TestAClass(),
                new TestBClass()
            };

            stopwatch.Restart();
            for (int i = 0; i < loops; i++)
            {
                for (int j = 0; j < classes.Length; j++)
                {
                    var c = classes[j];
                    if (c is TestBClass b)
                    {

                    }
                }
            }
            stopwatch.Stop();

            Console.WriteLine("Type check: {0}", stopwatch.ElapsedMilliseconds);

            stopwatch.Restart();
            for (int i = 0; i < loops; i++)
            {
                for (int j = 0; j < classes.Length; j++)
                {
                    var c = classes[j];
                    if (c.Type == (ushort)TestType.B)
                    {
                        var b = c as TestBClass;
                    }
                }
            }
            stopwatch.Stop();

            Console.WriteLine("Value check: {0}", stopwatch.ElapsedMilliseconds);
        }

        private class OldBitWriter
        {
            /// <summary>
            /// The default buffer length of BitWriter
            /// </summary>
            private const int Default_Length = 1024;

            /// <summary>
            /// Scratch bits used to hold currently written bits
            /// </summary>
            private ulong scratch = 0;

            /// <summary>
            /// The amount of bits still in the scratch value
            /// </summary>
            private int scratchBits = 0;

            /// <summary>
            /// The amount of bits written into the buffer
            /// </summary>
            private uint bitsWritten = 0;

            /// <summary>
            /// Buffer of already written bits
            /// </summary>
            private uint[] buffer = new uint[Default_Length];

            public long Length => (bitsWritten + scratchBits + 7) / 8;

            /// <summary>
            /// Expands the buffer by the default length
            /// </summary>
            private void ExpandBuffer()
            {
                uint[] newBuffer = new uint[buffer.Length + Default_Length];
                System.Buffer.BlockCopy(buffer, 0, newBuffer, 0, sizeof(uint) * buffer.Length);
                buffer = newBuffer;
            }

            /// <summary>
            /// Adds bits to the bit buffer
            /// </summary>
            /// <param name="bits"></param>
            private void AddToBuffer(uint bits)
            {
                uint index = bitsWritten / 32;
                if (index >= buffer.Length)
                {
                    ExpandBuffer();
                }

                buffer[index] = bits;
            }

            /// <summary>
            /// Flushes the last bits from the scratch. Only flushes a maximum of 32 bits
            /// </summary>
            /// <returns></returns>
            private int FlushScratch()
            {
                int amount = Math.Min(scratchBits, 32);
                uint bits = (uint)scratch;

                scratch >>= amount;
                scratchBits -= amount;

                AddToBuffer(bits);
                return amount;
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

            private void WriteArrayLength(uint length)
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
                scratch = 0;
                scratchBits = 0;
                bitsWritten = 0;
            }

            /// <summary>
            /// Returns a buffer containing all writen data.
            /// </summary>
            /// <returns></returns>
            public byte[] GetData()
            {
                uint bitCount = bitsWritten + (uint)scratchBits;
                var data = new byte[(int)((bitCount + 7) / 8)];
                int index = 0;
                if (bitsWritten > 0)
                {
                    int count = (int)(bitsWritten / 8);
                    Buffer.BlockCopy(buffer, 0, data, index, count);
                    index += count;
                }
                if (scratchBits > 0)
                {
                    int scratchBytes = (scratchBits + 7) / 8;
                    for (int i = 0; i < scratchBytes; i++)
                    {
                        byte b = (byte)(scratch >> (i * 8));
                        data[index] = b;
                        index++;
                    }
                    /*
                    if (scratchBytes < 4 || !BitConverter.IsLittleEndian)
                    {
                        for (int i = scratchBytes - 1; i >= 0; i--)
                        {
                            byte b = (byte)(scratch >> (i * 8));
                            data[index] = b;
                            index++;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < scratchBytes; i++)
                        {
                            byte b = (byte)(scratch >> (i * 8));
                            data[index] = b;
                            index++;
                        }
                    }
                    */
                }
                return data;
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

                scratch |= (cleanBits << scratchBits);
                scratchBits += length;

                if (scratchBits < 32) return;
                bitsWritten += (uint)FlushScratch();
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
        }
    }
}
