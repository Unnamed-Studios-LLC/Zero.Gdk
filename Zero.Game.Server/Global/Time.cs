using System.Diagnostics;

namespace Zero.Game.Server
{
    public static class Time
    {
        public unsafe struct MethodDurations
        {
            private fixed long Times[9];

            public long SynchronizationContext  { get => Times[0]; set => Times[0] = value; }
            public long AddRemoveWorlds         { get => Times[1]; set => Times[1] = value; }
            public long AddRemoveConnections    { get => Times[2]; set => Times[2] = value; }
            public long UpdateTasks             { get => Times[3]; set => Times[3] = value; }
            public long ReceiveData             { get => Times[4]; set => Times[4] = value; }
            public long UpdateWorlds            { get => Times[5]; set => Times[5] = value; }
            public long UpdateViews             { get => Times[6]; set => Times[6] = value; }
            public long SendData                { get => Times[7]; set => Times[7] = value; }
            public long WaitNext                { get => Times[8]; set => Times[8] = value; }

            public void Normalize(int durationMs)
            {
                var durationFrequency = (float)(durationMs * (Stopwatch.Frequency / 1000));
                for (int i = 0; i < 9; i++)
                {
                    Times[i] = (long)(Times[i] / durationFrequency * 100);
                }
            }
        }

        public static int Delta { get; internal set; }
        public static float DeltaF => Delta / 1000f;

        public static long LastUpdateDuration { get; internal set; }
        public static MethodDurations LastUpdateMethods { get; internal set; }

        public static long TargetDelta { get; internal set; }
        public static float TargetDeltaF => TargetDelta / 1000f;

        public static long Total { get; internal set; }
        public static float TotalF => Total / 1000f;
    }
}
