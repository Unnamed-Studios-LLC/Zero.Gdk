using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using Zero.Game.Server;
using Zero.Game.Shared;

namespace Zero.Game.Tests
{
    public unsafe class EcsTests
    {
        private const int EntityCount = 1000;
        private const int GroupCount = EntityCount / 4;

        private struct TestComponentA
        {
            public uint Value;
        }

        private struct TestComponentB
        {
            public uint Value1;
            public uint Value2;
            public uint Value3;
        }

        private struct TestComponentC
        {
            public fixed uint Values[3];
        }

        private struct TestComponentD
        {
            public uint Value;
        }

        private Entities _entities;

        private EntityLayout _layoutA;
        private EntityLayout _layoutB;
        private EntityLayout _layoutC;

        private readonly List<uint> _ids = new();

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _entities = new Entities();

            var defaultC = new TestComponentC();
            defaultC.Values[0] = 1;
            defaultC.Values[1] = 2;
            defaultC.Values[2] = 3;

            _layoutA = new EntityLayout()
                .Define(new TestComponentA { Value = 123 })
                .Define(defaultC)
                ;

            _layoutB = new EntityLayout()
                .Define(new TestComponentB { Value1 = 111, Value2 = 222, Value3 = 333 })
                .Define<TestComponentD>()
                ;

            _layoutC = new EntityLayout()
                .Define<TestComponentA>()
                .Define<TestComponentD>()
                ;

            // pre warm the entity buffers
            Setup();
            Cleanup();
        }

        [OneTimeTearDown]
        public void OneTimeCleanup()
        {
            _entities.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            for (int i = 0; i < EntityCount / 4; i++)
            {
                _ids.Add(_entities.CreateEntity(_layoutA));
                _ids.Add(_entities.CreateEntity(_layoutB));
                _ids.Add(_entities.CreateEntity(_layoutC));
            }
        }

        [TearDown]
        public void Cleanup()
        {
            for (int i = 0; i < _ids.Count; i++)
            {
                _entities.DestroyEntity(_ids[i]);
            }
            _ids.Clear();
        }

        [Test]
        public void LayoutDefaults()
        {
            _entities.ForEach((ref TestComponentB b, ref TestComponentD d) =>
            {
                Assert.AreEqual(111u, b.Value1);
                Assert.AreEqual(222u, b.Value2);
                Assert.AreEqual(333u, b.Value3);
            });

            _entities.ForEach((ref TestComponentA a, ref TestComponentC c) =>
            {
                Assert.AreEqual(1u, c.Values[0]);
                Assert.AreEqual(2u, c.Values[1]);
                Assert.AreEqual(3u, c.Values[2]);
            });
        }

        [Test]
        public void ComponentConsistency()
        {
            _entities.ForEach((uint entityId, ref TestComponentA a, ref TestComponentD d) =>
            {
                Assert.AreEqual(0u, a.Value);
                Assert.AreEqual(0u, d.Value);
                a.Value += entityId;
            });

            _entities.DestroyEntity(_ids[^1]);
            _ids.RemoveAt(_ids.Count - 1);
            _ids.Add(_entities.CreateEntity(_layoutA));

            _entities.ParallelForEach((uint entityId, ref TestComponentA a, ref TestComponentD d) =>
            {
                Assert.AreEqual(entityId, a.Value);
                Assert.AreEqual(0u, d.Value);
            });

            _entities.DestroyEntity(_ids[^1]);
            _ids.RemoveAt(_ids.Count - 1);
            _ids.Add(_entities.CreateEntity(_layoutA));

            _entities.ForEach((uint entityId, ref TestComponentA a, ref TestComponentD d) =>
            {
                Assert.AreEqual(entityId, a.Value);
                Assert.AreEqual(0u, d.Value);
                a.Value += entityId;
            });

            _entities.DestroyEntity(_ids[^1]);
            _ids.RemoveAt(_ids.Count - 1);
            _ids.Add(_entities.CreateEntity(_layoutA));

            _entities.ForEach((uint entityId, ref TestComponentA a, ref TestComponentD d) =>
            {
                Assert.AreEqual(entityId * 2, a.Value);
                Assert.AreEqual(0u, d.Value);
            });
        }

        [Test]
        public void ForEachCountAccuracy()
        {
            int count = 0;
            _entities.ForEach((ref TestComponentA a) =>
            {
                count++;
            });

            _entities.ForEach((ref TestComponentD d) =>
            {
                count++;
            });

            var expected = GroupCount * 4;
            Assert.AreEqual(expected, count);
        }

        [Test]
        public void ParallelForEachCountAccuracy()
        {
            int count = 0;
            _entities.ForEach((ref TestComponentA a) =>
            {
                Interlocked.Increment(ref count);
            });

            _entities.ForEach((ref TestComponentD d) =>
            {
                Interlocked.Increment(ref count);
            });

            var expected = GroupCount * 4;
            Assert.AreEqual(expected, count);
        }
    }
}
