using System.Collections.Generic;

namespace Zero.Game.Server
{
    public class LockingList<T>
    {
        private readonly List<T> _list;

        public LockingList(int initialCapacity)
        {
            _list = new(initialCapacity);
        }

        public void Add(T value)
        {
            lock (_list)
            {
                _list.Add(value);
            }
        }

        public bool TryPop(out T value)
        {
            lock (_list)
            {
                if (_list.Count == 0)
                {
                    value = default;
                    return false;
                }
                value = _list[^1];
                _list.RemoveAt(_list.Count - 1);
                return true;
            }
        }
    }
}
