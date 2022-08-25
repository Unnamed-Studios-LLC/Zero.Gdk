using System.Collections.Generic;

namespace Zero.Game.Server
{
    internal static class DataSnapshotCache
    {
        public readonly static Stack<DataSnapshot> _snapshots = new Stack<DataSnapshot>();

        public static DataSnapshot Get()
        {
            if (_snapshots.Count == 0)
            {
                return new DataSnapshot();
            }

            return _snapshots.Pop();
        }

        public static void Return(DataSnapshot snapshot)
        {
            snapshot.ReturnItemsToCache();
            _snapshots.Push(snapshot);
        }
    }
}
