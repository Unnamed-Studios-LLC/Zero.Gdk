using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Zero.Game.Benchmark
{
    [MemoryDiagnoser]
    public unsafe class Program
    {
        private const int _count = 100;
        private const int _funcCount = 10000;

        private readonly Dictionary<long, int> _dict = new();
        private readonly List<KeyValuePair<long, int>> _list = new();

        [GlobalSetup]
        public unsafe void Setup()
        {
            for (int i = 0; i < _count; i++)
            {
                _dict[Random.Shared.Next()] = Random.Shared.Next();
            }
        }

        [GlobalCleanup]
        public unsafe void Cleanup()
        {

        }

        [Benchmark]
        public int OrderBy()
        {
            int c = 0;
            var ordered = _dict.OrderBy(x => x.Value);
            foreach (var entry in ordered)
            {
                c += entry.Value;
            }
            return c;
        }

        [Benchmark]
        public int Sort()
        {
            int c = 0;
            _list.AddRange(_dict);
            _list.Sort((a, b) => b.Value - a.Value);
            var span = CollectionsMarshal.AsSpan(_list);
            foreach (ref var entry in span)
            {
                c += entry.Value;
            }
            _list.Clear();
            return c;
        }

        static void Main(string[] args)
        {
#if DEBUG
            var p = new Program();
            p.Setup();
            p.Sort();
            p.Cleanup();
#else
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
#endif
        }
    }
}
