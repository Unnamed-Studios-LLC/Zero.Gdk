namespace Zero.Game.Server
{
    public static class Time
    {
        public static int Delta { get; internal set; }

        public static long LastTickDuration { get; internal set; }

        public static long Total { get; internal set; }
    }
}
