using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using Zero.Game.Server;
using Zero.Game.Shared;

namespace Zero.Game.Benchmark
{
    public unsafe class Program
    {
        private const int EntityCount = 1000;

        private struct TestComponentA
        {
            public uint Value;
        }

        private struct TestComponentB
        {
            public fixed uint Values[200];
        }

        private struct TestComponentC
        {
            public fixed uint Values[3];
        }

        private struct TestComponentD
        {
            public uint Value;
        }


        private const int _count = 100;
        private const int _funcCount = 10000;

        private readonly byte[] _bytesA = new byte[_count];
        private readonly byte[] _bytesB = new byte[_count];

        private TestComponentB[] _data = new TestComponentB[_count];

        [GlobalSetup]
        public unsafe void Setup()
        {

        }

        [GlobalCleanup]
        public unsafe void Cleanup()
        {

        }

        [Benchmark]
        public void MemoryCopy()
        {
            fixed (byte* src = _bytesA)
            fixed (byte* dst = _bytesB)
            {
                Buffer.MemoryCopy(src, dst, _count, _count);
            }
        }

        static void Main(string[] args)
        {
#if DEBUG
            var p = new Program();
            p.Setup();

            p.Cleanup();
#else
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
#endif
        }
    }
}
