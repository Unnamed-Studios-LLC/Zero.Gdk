namespace Zero.Game.Shared
{
    public static class Every
    {
        public const long Once = -1;

        public static bool Interval(long delta, long interval, ref long value)
        {
            var once = interval < 0;
            if (value < 0 &&
                once)
            {
                return false;
            }

            value -= delta;
            var activated = value <= 0;
            if (activated)
            {
                value = once ? -1 : interval + value % interval;
            }
            return activated;
        }
    }
}
