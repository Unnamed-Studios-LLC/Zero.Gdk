using System.Collections.Generic;
using System.Linq;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public sealed class World : Entity
    {
        private readonly Dictionary<string, string> _emptyData = new();

        private readonly Dictionary<uint, Connection> _connections = new();
        private readonly Dictionary<uint, Entity> _entities = new();
        private readonly ComponentSystemCollection _componentSystemCollection;
        private uint _nextEntityId;
        private Entity[] _entityArray;
        
        public IReadOnlyDictionary<string, string> Data { get; }

        internal override ObjectType ObjectType => ObjectType.World;

        internal World(uint id, Dictionary<string, string> data)
        {
            Id = id;
            Data = data ?? _emptyData;
            _componentSystemCollection = ServerDomain.Schema?.CreateComponentSystemCollection(id);
            AddToWorld(this);
        }

        public void AddEntity(Entity entity)
        {
            entity.Id = GetEntityId();

            _entities.Add(entity.Id, entity);
            entity.AddToWorld(this);
            _entityArray = null;
        }

        public Entity GetEntity(uint id)
        {
            if (!_entities.TryGetValue(id, out var entity))
            {
                return null;
            }
            return entity;
        }

        public IEnumerable<Entity> GetEntities()
        {
            _entityArray ??= _entities.Values.ToArray();
            return _entityArray;
        }

        public void RemoveEntity(uint id)
        {
            var entity = GetEntity(id);
            RemoveEntity(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            if (entity == null ||
                entity.Id == 0 ||
                entity.World != this)
            {
                return;
            }

            entity.RemoveFromWorld();
            _entities.Remove(entity.Id);
            _entityArray = null;
        }

        private uint GetEntityId()
        {
            do
            {
                _nextEntityId++;
            }
            while (_nextEntityId == 0 || _entities.ContainsKey(_nextEntityId));
            return _nextEntityId;
        }

        internal void AddConnection(Connection connection)
        {
            _connections.Add(connection.Id, connection);
            connection.ClearView();
            connection.AddToWorld(this);
        }

        internal void AddToNode()
        {
            AddToWorld(this);
        }

        /*
        internal void DeregisterComponent(Component component)
        {
            _componentSystemCollection.Deregister(component);
        }
        */

        internal void RegisterComponent(Component component)
        {
            if (component.RegisteredWorldId == Id)
            {
                return;
            }

            _componentSystemCollection.Register(component);
            component.RegisteredWorldId = Id;
        }

        internal void RemoveConnection(Connection connection)
        {
            connection.RemoveFromWorld();
            _connections.Remove(connection.Id);
        }

        internal void RemoveFromNode()
        {
            InternalActive = false;
            foreach (var connection in _connections.Values)
            {
                connection.Close();
            }
            _connections.Clear();
            RemoveFromWorld();
        }

        internal void UpdateAll()
        {
            if (!IsActive)
            {
                return;
            }

            _componentSystemCollection.UpdateAll();
        }

        internal void ViewUpdateAll()
        {
            if (!IsActive)
            {
                return;
            }
            _componentSystemCollection.ViewUpdateAll();
        }
    }
}
