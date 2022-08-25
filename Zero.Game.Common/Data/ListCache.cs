using System.Collections.Concurrent;
using System.Collections.Generic;
using Zero.Game.Shared;

namespace Zero.Game.Common
{
    public static class ListCache
    {
        private static readonly ConcurrentQueue<List<IData>> _dataLists = new ConcurrentQueue<List<IData>>();
        private static readonly ConcurrentQueue<uint[]> _uintArrays = new ConcurrentQueue<uint[]>();
        private static readonly ConcurrentQueue<List<ViewAction>> _viewActionLists = new ConcurrentQueue<List<ViewAction>>();

        public static List<IData> GetDataList()
        {
            if (_dataLists.TryDequeue(out var list))
            {
                return list;
            }

            return new List<IData>(256);
        }

        public static uint[] GetUintArray()
        {
            if (_uintArrays.TryDequeue(out var list))
            {
                return list;
            }

            return new uint[10_000];
        }

        public static List<ViewAction> GetViewActionList()
        {
            if (_viewActionLists.TryDequeue(out var list))
            {
                return list;
            }

            return new List<ViewAction>(256);
        }

        public static void ReturnDataList(List<IData> list)
        {
            list.Clear();
            _dataLists.Enqueue(list);
        }

        public static void ReturnUintArray(uint[] list)
        {
            _uintArrays.Enqueue(list);
        }

        public static void ReturnViewActionList(List<ViewAction> list)
        {
            list.Clear();
            _viewActionLists.Enqueue(list);
        }
    }
}
