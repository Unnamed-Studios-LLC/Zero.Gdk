using System.Collections.Generic;
using Zero.Game.Common;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    internal class DataState
    {
        private class DataIndex
        {
            public int Private { get; set; } = -1;
            public int PrivateUpdated { get; set; } = -1;
            public int Public { get; set; } = -1;
            public int PublicUpdated { get; set; } = -1;
        }

        private readonly Dictionary<ushort, DataIndex> _indices = new();

        private bool _privateEnabled = false;

        private List<IData> _privateData;// = ListCache.GetDataList();
        private List<IData> _privateDataUpdated;// = ListCache.GetDataList();
        private List<IData> _privateDataNext;// = ListCache.GetDataList();
        private List<IData> _publicData = ListCache.GetDataList();
        private List<IData> _publicDataUpdated = ListCache.GetDataList();
        private List<IData> _publicDataNext = ListCache.GetDataList();

        public IData GetData(ushort type)
        {
            if (!_indices.TryGetValue(type, out var index) ||
                index.Private < 0)
            {
                return null;
            }

            return _privateData[index.Private];
        }

        public DataSnapshot GetSnapshot(ObjectType objectType, uint id)
        {
            var snapshot = DataSnapshotCache.Get();
            snapshot.Assign(objectType, id,
                _privateData,
                _privateDataUpdated,
                _publicData,
                _publicDataUpdated
            );

            var dataLoopList = _privateEnabled ? _privateData : _publicData;
            for (int i = 0; i < dataLoopList.Count; i++)
            {
                var data = dataLoopList[i];
                var definition = ServerDomain.Schema.GetDataDefinition(data.Type);
                var index = _indices[data.Type];

                index.PrivateUpdated = -1;
                index.PublicUpdated = -1;

                if (!definition.Persist ||
                    definition.IsDefault(data))
                {
                    index.Private = -1;

                    if (index.Public >= 0)
                    {
                        index.Public = -1;
                    }
                }
                else
                {
                    if (_privateEnabled)
                    {
                        index.Private = _privateDataNext.Count;
                        _privateDataNext.Add(data);
                    }

                    if (index.Public >= 0)
                    {
                        index.Public = _publicDataNext.Count;
                        _publicDataNext.Add(data);
                    }
                }
            }

            _privateData = _privateDataNext;
            _privateDataUpdated = ListCache.GetDataList();
            _privateDataNext = ListCache.GetDataList();
            _publicData = _publicDataNext;
            _publicDataUpdated = ListCache.GetDataList();
            _publicDataNext = ListCache.GetDataList();

            return snapshot;
        }

        public void PushPrivate(IData data)
        {
            if (!_privateEnabled)
            {
                EnablePrivate();
                _privateEnabled = true;
            }

            var index = GetIndex(data.Type);
            var definition = ServerDomain.Schema.GetDataDefinition(data.Type);
            PushPrivate(data, index, definition);
        }

        public void PushPublic(IData data)
        {
            var definition = ServerDomain.Schema.GetDataDefinition(data.Type);
            var index = GetIndex(data.Type);
            if (index.Public >= 0)
            {
                _publicData[index.Public] = data;
            }
            else if (!definition.IsDefault(data))
            {
                index.Public = _publicData.Count;
                _publicData.Add(data);
            }

            if (index.PublicUpdated >= 0)
            {
                _publicDataUpdated[index.PublicUpdated] = data;
            }
            else
            {
                index.PublicUpdated = _publicDataUpdated.Count;
                _publicDataUpdated.Add(data);
            }
            PushPrivate(data, index, definition);
        }

        private void EnablePrivate()
        {
            _privateData = ListCache.GetDataList();
            _privateDataUpdated = ListCache.GetDataList();
            _privateDataNext = ListCache.GetDataList();

            for (int i = 0; i < _publicData.Count; i++)
            {
                var data = _publicData[i];
                var index = _indices[data.Type];
                index.Private = _privateData.Count;
                _privateData.Add(data);
            }

            for (int i = 0; i < _publicDataUpdated.Count; i++)
            {
                var data = _publicDataUpdated[i];
                var index = _indices[data.Type];
                index.PrivateUpdated = _privateDataUpdated.Count;
                _privateDataUpdated.Add(data);
            }
        }

        private DataIndex GetIndex(ushort type)
        {
            if (!_indices.TryGetValue(type, out var index))
            {
                index = new DataIndex();
                _indices.Add(type, index);
            }
            return index;
        }

        private void PushPrivate(IData data, DataIndex index, DataDefinition definition)
        {
            if (!_privateEnabled)
            {
                return;
            }

            if (index.Private >= 0)
            {
                _privateData[index.Private] = data;
            }
            else if (!definition.IsDefault(data))
            {
                index.Private = _privateData.Count;
                _privateData.Add(data);
            }

            if (index.PrivateUpdated >= 0)
            {
                _privateDataUpdated[index.PrivateUpdated] = data;
            }
            else
            {
                index.PrivateUpdated = _privateDataUpdated.Count;
                _privateDataUpdated.Add(data);
            }
        }
    }
}
