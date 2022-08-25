using System.Collections.Generic;
using Zero.Game.Common;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    internal class View
    {
        private Dictionary<uint, Entity> _entitiesInView = new();
        private Dictionary<uint, Entity> _lastEntitiesInView = new();
        private readonly HashSet<uint> _authorityEntities = new();
        private List<ViewAction> _viewActions = new();
        private uint _nextBatchId = 0;
        private uint _worldId = 0;
        private long _lastBatchTime;

        public void Clear()
        {
            _entitiesInView.Clear();
            _lastEntitiesInView.Clear();
            _authorityEntities.Clear();
        }

        public void GiveAuthority(uint entityId)
        {
            _authorityEntities.Add(entityId);
        }

        public bool HasAuthority(uint entityId)
        {
            return _authorityEntities.Contains(entityId);
        }

        public void Transfer(string ip, ushort port, string key)
        {
            var action = ViewActionCache.GetTransfer(ip, port, key);
            action.Aquire();
            _viewActions.Add(action);
        }

        public void Update(World world)
        {
            ProcessWorld(world);
            GatherUpdatedData();
        }

        public void ViewUpdate(World world, IEnumerable<Entity> entities)
        {
            ProcessWorld(world);
            SwapViewDictionaries();
            UpdateEntitiesInView(entities);
            AddRemovedEntities();
        }

        private void AddData(DataContainer container)
        {
            var snapshot = container.GetSnapshot(Server.Update.Id);
            var hasAuthority = container.ObjectType == ObjectType.Entity && _authorityEntities.Contains(container.Id);
            var update = snapshot.GetUpdate(hasAuthority, true);
            update.Aquire();
            _viewActions.Add(update);
        }

        private void AddDataUpdated(DataContainer container)
        {
            var snapshot = container.GetSnapshot(Server.Update.Id);
            var hasAuthority = container.ObjectType == ObjectType.Entity && _authorityEntities.Contains(container.Id);
            var update = snapshot.GetUpdate(hasAuthority, false);
            if (update.Data.Count == 0)
            {
                return;
            }
            update.Aquire();
            _viewActions.Add(update);
        }

        private void AddRemovedEntities()
        {
            if (_lastEntitiesInView.Count == 0)
            {
                return;
            }

            var removedList = ListCache.GetUintArray();
            var keys = _lastEntitiesInView.Keys;
            keys.CopyTo(removedList, 0);

            var remove = ViewActionCache.GetRemove((uint)keys.Count, removedList);
            remove.Aquire();
            _viewActions.Add(remove);
        }

        private void GatherUpdatedData()
        {
            foreach (var entity in _entitiesInView.Values)
            {
                AddDataUpdated(entity);
            }
        }

        private void ProcessEntity(Entity entity)
        {
            if (_entitiesInView.ContainsKey(entity.Id))
            {
                return; // already processed
            }
            _entitiesInView.Add(entity.Id, entity);

            if (!_lastEntitiesInView.ContainsKey(entity.Id))
            {
                // create
                AddData(entity);
            }
            else
            {
                _lastEntitiesInView.Remove(entity.Id);
                AddDataUpdated(entity);
            }
        }

        private void ProcessWorld(World world)
        {
            if (_worldId != world.Id)
            {
                _worldId = world.Id;
                AddData(world);
            }
            else
            {
                AddDataUpdated(world);
            }
        }

        private void SwapViewDictionaries()
        {
            var temp = _entitiesInView;
            _entitiesInView = _lastEntitiesInView;
            _lastEntitiesInView = temp;
            _entitiesInView.Clear();
        }

        private void UpdateEntitiesInView(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                ProcessEntity(entity);
            }
        }

        internal ViewActionBatch GetBatch()
        {
            _lastBatchTime = Time.Total;
            var batch = ViewActionBatchCache.Get(_nextBatchId++, 0, _viewActions);
            _viewActions = ListCache.GetViewActionList();
            return batch;
        }

        internal bool HasBatch()
        {
            return _viewActions.Count > 0 ||
                Time.Total - _lastBatchTime >= 1000;
        }
    }
}
