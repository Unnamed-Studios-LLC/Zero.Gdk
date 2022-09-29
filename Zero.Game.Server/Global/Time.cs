namespace Zero.Game.Server
{
    public static class Time
    {
        public static int Delta { get; internal set; }
        public static float DeltaF => Delta / 1000f;

        public static long LastUpdateDuration { get; internal set; }

        public static long Total { get; internal set; }
        public static float TotalF => Total / 1000f;
    }
}
