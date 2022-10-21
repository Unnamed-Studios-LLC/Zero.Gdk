using System.Diagnostics;
using System.Threading;

namespace Zero.Game.Server
{
    internal sealed class Ticker
    {
        private const int PrecisionDelay = 2;

        private readonly ManualResetEvent _waitEvent = new(false);
        private readonly Stopwatch _stopwatch = new();
        private readonly uint _tickInterval;

        public Ticker(uint tickInterval)
        {
            _tickInterval = tickInterval;
            Delta = tickInterval;
        }

        public uint Delta { get; private set; }
        public int LastUpdateDuration { get; private set; }
        public long Total { get; private set; }

        public void Start()
        {
            _stopwatch.Start();
        }

        public void Stop()
        {
            _waitEvent.Set();
        }

        public void WaitNext()
        {
            var intervalProgress = _stopwatch.ElapsedMilliseconds - Total;
            LastUpdateDuration = (int)intervalProgress;
            var delay = (int)(_tickInterval - intervalProgress);
            if (delay > 0)
            {
                // wait larger ms interval
                if (delay > PrecisionDelay)
                {
                    _waitEvent.WaitOne(delay - PrecisionDelay);
                }

                // spin wait the last bits
                do
                {
                    intervalProgress = _stopwatch.ElapsedMilliseconds - Total;
                    delay = (int)(_tickInterval - intervalProgress);
                }
                while (delay > 0);
            }

            Delta = (uint)(_stopwatch.ElapsedMilliseconds - Total);
            Total = _stopwatch.ElapsedMilliseconds;
        }
    }
}
