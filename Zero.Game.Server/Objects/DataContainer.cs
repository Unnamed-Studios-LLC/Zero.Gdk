using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public abstract class DataContainer
    {
        private readonly DataState _dataState = new();
        private DataSnapshot _snapshot;
        private ulong _snapshotTickId;

        public uint Id { get; internal set; }

        internal abstract ObjectType ObjectType { get; }

        public IData GetData(ushort type)
        {
            return _dataState.GetData(type);
        }

        public T GetData<T>() where T : IData
        {
            T def = default;
            var data = _dataState.GetData(def.Type);
            if (data == null)
            {
                return def;
            }
            return (T)data;
        }

        public void PushPrivate(IData data)
        {
            _dataState.PushPrivate(data);
        }

        public void PushPublic(IData data)
        {
            _dataState.PushPublic(data);
        }

        internal DataSnapshot GetSnapshot(ulong tickId)
        {
            if (_snapshotTickId != tickId)
            {
                if (_snapshot != null)
                {
                    DataSnapshotCache.Return(_snapshot);
                }

                _snapshot = _dataState.GetSnapshot(ObjectType, Id);
                _snapshotTickId = tickId;
            }

            return _snapshot;
        }
    }
}
