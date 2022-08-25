using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    internal static class Ticker
    {
        private const int PrecisionDelay = 2;

        private static bool _running;
        private static readonly ManualResetEvent _waitEvent = new(false);
        private static readonly ManualResetEvent _stoppedEvent = new(false);

        public static void Run(Action tickAction)
        {
            if (tickAction is null)
            {
                throw new ArgumentNullException(nameof(tickAction));
            }

            _running = true;
            var msInterval = ServerDomain.Options.TickIntervalMs;
            long lastElapsed = 0;
            long elapsed;
            long tickElapsed;
            int delay;
            var stopwatch = new Stopwatch();

            var elapsedList = new List<long>();
            long lastCapacity = 0;

            stopwatch.Start();
            while (_running)
            {
                elapsed = stopwatch.ElapsedMilliseconds;

                Update.Id++;
                Time.Delta = (int)(elapsed - lastElapsed);
                Time.Total = elapsed;
                lastElapsed = elapsed;

                tickAction();

                tickElapsed = stopwatch.ElapsedMilliseconds - elapsed;
                Time.LastTickDuration = tickElapsed;
                if (tickElapsed > msInterval)
                {
                    ServerDomain.InternalLog(LogLevel.Trace, "Tick lagged! {0}ms", tickElapsed);
                }
                delay = (int)(msInterval - tickElapsed);
                if (delay > 0)
                {
                    if (delay > PrecisionDelay)
                    {
                        _waitEvent.WaitOne(delay - PrecisionDelay);
                    }

                    tickElapsed = stopwatch.ElapsedMilliseconds - elapsed;
                    elapsedList.Add(tickElapsed);
                    if (stopwatch.ElapsedMilliseconds - lastCapacity >= 1000)
                    {
                        UpdateCapacity(elapsedList);
                        elapsedList.Clear();
                    }

                    tickElapsed = stopwatch.ElapsedMilliseconds - elapsed;
                    delay = (int)(msInterval - tickElapsed);

                    while (delay > 0)
                    {
                        tickElapsed = stopwatch.ElapsedMilliseconds - elapsed;
                        delay = (int)(msInterval - tickElapsed);
                    }
                }
            }
            _stoppedEvent.Set();
        }

        public static void Stop()
        {
            _running = false;
            _waitEvent.Set();
            _stoppedEvent.WaitOne();
        }

        private static void UpdateCapacity(List<long> elapsedList)
        {
            long total = 0;
            for (int i = 0; i < elapsedList.Count; i++)
            {
                total += elapsedList[i];
            }
            var average = total / elapsedList.Count;
            ZeroServer.CapacityInternal = (int)((average * 100) / ServerDomain.Options.TickIntervalMs);
        }
    }
}
