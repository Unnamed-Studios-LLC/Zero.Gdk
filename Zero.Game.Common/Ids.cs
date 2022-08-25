namespace Zero.Game.Common
{
    public static class Ids
    {
        public static int GetDifference(uint a, uint b)
        {
            return (int)(a - b);
        }

        public static ulong GetFullId(uint id, uint series)
        {
            return ((ulong)series << 32) | id;
        }

        public static uint GetSeries(uint id, uint refId, uint refSeries)
        {
            if (id == refId)
            {
                return refSeries;
            }

            var difference = (int)(id - refId);
            if (difference > 0)
            {
                return id > refId ? refSeries : (refSeries + 1);
            }
            else
            {
                return id < refId ? refSeries : (refSeries - 1);
            }
        }
    }
}
