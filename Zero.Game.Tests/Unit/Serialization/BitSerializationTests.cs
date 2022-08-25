using NUnit.Framework;
using System;
using Zero.Game.Common;

namespace Zero.Game.Tests.Unit.Serialization
{
    public class BitSerializationTests
    {
        private BitReader _reader;
        private BitWriter _writer;

        [SetUp]
        public void Setup()
        {
            _writer = new BitWriter(new byte[10_000]);
        }

        [Test]
        public void Bool_Size_Test()
        {
            // arrange
            int targetByteSize = 1;

            // act
            for (int i = 0; i < 8; i++)
            {
                _writer.Write(true);
            }

            var data = _writer.GetBuffer();

            // assert
            Assert.AreEqual(targetByteSize, data.Size);
        }

        [Test]
        public void Serialize_Large_Test_1()
        {
            // arrange
            int loops = 100;

            var a = true;
            int b = 123;
            string c = "this is a large string test";
            byte d = 86;
            double e = 3456.8576;

            // act
            for (int i = 0; i < loops; i++)
            {
                _writer.Write(a);
                _writer.Write(b);
                _writer.Write(c);
                _writer.Write(d);
                _writer.Write(e);
            }

            var buffer = _writer.GetBuffer();
            _reader = new BitReader(buffer.Data, 0, buffer.Size);

            // assert
            for (int i = 0; i < loops; i++)
            {
                Assert.AreEqual(a, _reader.ReadBool());
                Assert.AreEqual(b, _reader.ReadInt32());
                Assert.AreEqual(c, _reader.ReadUtf());
                Assert.AreEqual(d, _reader.ReadUInt8());
                Assert.AreEqual(e, _reader.ReadDouble());
            }
        }

        [Test]
        public void Serialize_Small_Test_1()
        {
            // arrange
            var a = true;
            var b = false;
            var c = true;
            var d = true;
            var e = false;

            // act
            _writer.Write(a);
            _writer.Write(b);
            _writer.Write(c);
            _writer.Write(d);
            _writer.Write(e);

            var buffer = _writer.GetBuffer();
            _reader = new BitReader(buffer.Data, 0, buffer.Size);

            // assert
            Assert.AreEqual(a, _reader.ReadBool());
            Assert.AreEqual(b, _reader.ReadBool());
            Assert.AreEqual(c, _reader.ReadBool());
            Assert.AreEqual(d, _reader.ReadBool());
            Assert.AreEqual(e, _reader.ReadBool());
        }

        [Test]
        public void Serialize_Small_Test_2()
        {
            // arrange
            var a = true;
            int b = 123;
            string c = "test";
            byte d = 86;
            double e = 3456.8576;

            // act
            _writer.Write(a);
            _writer.Write(b);
            _writer.Write(c);
            _writer.Write(d);
            _writer.Write(e);

            var buffer = _writer.GetBuffer();
            _reader = new BitReader(buffer.Data, 0, buffer.Size);

            // assert
            Assert.AreEqual(a, _reader.ReadBool());
            Assert.AreEqual(b, _reader.ReadInt32());
            Assert.AreEqual(c, _reader.ReadUtf());
            Assert.AreEqual(d, _reader.ReadUInt8());
            Assert.AreEqual(e, _reader.ReadDouble());
        }

        [Test]
        public void Serialize_Small_Test_3()
        {
            // arrange
            uint a = 1;
            uint b = 2;
            ushort c = 1;
            uint d = 0;

            // act
            _writer.Write(a, 3);
            _writer.Write(b, 2);
            _writer.Write(c);
            _writer.Write(d, 3);

            var buffer = _writer.GetBuffer();
            _reader = new BitReader(buffer.Data, 0, buffer.Size);

            Assert.AreEqual(a, _reader.Read(3));
            Assert.AreEqual(b, _reader.Read(2));
            Assert.AreEqual(c, _reader.ReadUInt16());
            Assert.AreEqual(d, _reader.Read(3));
        }

        [Test]
        public void Serialize_Small_Test_4()
        {
            // arrange
            uint a = 1;
            uint b = 2;
            ushort c = 1;
            uint d = 0;
            float e = 4.12321f;
            uint f = 4;

            // act
            _writer.Write(a, 3);
            _writer.Write(b, 2);
            _writer.Write(c);
            _writer.Write(d, 3);
            _writer.Write(e);
            _writer.Write(f, 5);

            var buffer = _writer.GetBuffer();
            _reader = new BitReader(buffer.Data, 0, buffer.Size);

            Assert.AreEqual(a, _reader.Read(3));
            Assert.AreEqual(b, _reader.Read(2));
            Assert.AreEqual(c, _reader.ReadUInt16());
            Assert.AreEqual(d, _reader.Read(3));
            Assert.AreEqual(e, _reader.ReadFloat());
            Assert.AreEqual(f, _reader.Read(5));
        }

        [Test]
        public void Serialize_Small_Test_5()
        {
            // arrange
            uint actionType = 1;
            uint objectType = 2;
            uint entityId = 1;
            byte componentId = 1;
            ushort componentType = 1;
            uint actionsEnd = 31;

            // act
            _writer.Write(actionType, 3);
            _writer.Write(objectType, 2);
            _writer.Write(entityId);
            _writer.Write(componentId);
            _writer.Write(componentType);
            _writer.Write(actionsEnd, 5);

            var buffer = _writer.GetBuffer();
            _reader = new BitReader(buffer.Data, 0, buffer.Size);

            // assert
            Assert.AreEqual(actionType, _reader.Read(3));
            Assert.AreEqual(objectType, _reader.Read(2));
            Assert.AreEqual(entityId, _reader.ReadUInt32());
            Assert.AreEqual(componentId, _reader.ReadUInt8());
            Assert.AreEqual(componentType, _reader.ReadUInt16());
            Assert.AreEqual(actionsEnd, _reader.Read(5));
        }

        [Test]
        public void Serialize_Small_Test_6()
        {
            // arrange
            uint a = 31;

            // act
            _writer.Write(a, 5);
            
            var buffer = _writer.GetBuffer();
            _reader = new BitReader(buffer.Data, 0, buffer.Size);

            // assert
            Assert.AreEqual(a, _reader.Read(5));
        }

        [Test]
        public void Serialize_Small_Test_7()
        {
            // arrange
            ushort a = 0;
            uint b = 0;
            uint c = uint.MaxValue;
            uint d = 7;
            uint e = 1;
            ushort f = 1;
            uint g = 0;

            // act
            _writer.Write(a);
            _writer.Write(b);
            _writer.Write(c);
            _writer.Write(d, 3);
            _writer.Write(e);
            _writer.Write(f);
            _writer.Write(g, 3);

            var buffer = _writer.GetBuffer();
            var truncData = new byte[buffer.Data.Length - 2];
            Array.Copy(buffer.Data, 2, truncData, 0, truncData.Length);

            _reader = new BitReader(truncData);

            // assert
            Assert.AreEqual(b, _reader.ReadUInt32());
            Assert.AreEqual(c, _reader.ReadUInt32());
            Assert.AreEqual(d, _reader.Read(3));
            Assert.AreEqual(e, _reader.ReadUInt32());
            Assert.AreEqual(f, _reader.ReadUInt16());
            Assert.AreEqual(g, _reader.Read(3));
        }

        [TestCase((ushort)0)]
        [TestCase((ushort)1)]
        [TestCase((ushort)2)]
        [TestCase((ushort)3)]
        public void Serialize_Small_Test_8(ushort value)
        {
            // arrange

            // act
            _writer.Write(value);

            var buffer = _writer.GetBuffer();

            _reader = new BitReader(buffer.Data);

            // assert
            Assert.AreEqual(value, _reader.ReadUInt16());
        }

        [Test]
        public void Serialize_Array_Small()
        {
            // arrange
            var arraySize = 32;
            var array = new ushort[arraySize];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (ushort)i;
            }

            // act
            _writer.Write(array);

            var buffer = _writer.GetBuffer();
            _reader = new BitReader(buffer.Data, 0, buffer.Size);
            var readArray = _reader.ReadUInt16Array();

            // assert
            Assert.AreEqual(array.Length, readArray.Length);
            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(array[i], readArray[i]);
            }
        }

        [Test]
        public void Serialize_Array_Large()
        {
            // arrange
            var arraySize = 300;
            var array = new ushort[arraySize];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (ushort)i;
            }

            // act
            _writer.Write(array);

            var buffer = _writer.GetBuffer();
            _reader = new BitReader(buffer.Data, 0, buffer.Size);
            var readArray = _reader.ReadUInt16Array();

            // assert
            Assert.AreEqual(array.Length, readArray.Length);
            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(array[i], readArray[i]);
            }
        }
    }
}
