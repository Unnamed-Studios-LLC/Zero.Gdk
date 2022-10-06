using System;
using System.Collections.Generic;

namespace Zero.Game.Server
{
    public class World
    {
        private readonly List<ComponentSystem> _componentSystems = new();
        private readonly List<ComponentSystem> _componentSystemsAlt = new();
        private bool _parallel;

        public World(uint id, Dictionary<string, string> data)
        {
            Id = id;
            Data = data;
            EntityId = Entities.CreateEntity();
        }

        /// <summary>
        /// The world command buffer. Commands are executed after each component system is updated
        /// </summary>
        public CommandBuffer Commands { get; } = new();

        /// <summary>
        /// Data set when the world was created
        /// </summary>
        public Dictionary<string, string> Data { get; }

        /// <summary>
        /// Access to world entities
        /// </summary>
        public Entities Entities { get; } = new();

        /// <summary>
        /// Id of the entity created for this world
        /// </summary>
        public uint EntityId { get; }

        /// <summary>
        /// The unique Id of this world
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// Sets a max connection count for this world (-1, no max by default)
        /// </summary>
        public int MaxConnections { get; set; } = -1;

        /// <summary>
        /// If this world should update in parallel.
        /// </summary>
        public bool Parallel
        {
            get => _parallel;
            set
            {
                if (ParallelLocked)
                {
                    throw new Exception($"{nameof(Parallel)} cannot be altered after world load");
                }
                _parallel = value;
                Entities.RunningInParallel = value;
            }
        }

        /// <summary>
        /// A user-assignable state object
        /// </summary>
        public object State { get; set; }

        internal List<Connection> Connections { get; } = new List<Connection>();
        internal bool ConnectionsChanged { get; set; }
        internal bool HasMaxConnections => MaxConnections >= 0 && Connections.Count >= MaxConnections;
        internal bool ParallelLocked { get; set; }

        /// <summary>
        /// Adds a given component system to the world. Component systems cannot be added to multiple worlds
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="componentSystem"></param>
        public void AddSystem<T>(T componentSystem) where T : ComponentSystem
        {
            if (componentSystem.World != null)
            {
                throw new Exception("Component system already added to a world");
            }

            componentSystem.AddToWorld(this);
            _componentSystems.Add(componentSystem);
            Entities.SubscribeSystem(componentSystem);
        }

        /// <summary>
        /// Returns a component system matching the given type. Null is returned if no matching system is found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSystem<T>() where T : ComponentSystem
        {
            for (int i = 0; i < _componentSystems.Count; i++)
            {
                if (_componentSystems[i] is T typed)
                {
                    return typed;
                }
            }
            return default;
        }

        /// <summary>
        /// Removes a given component system if it exists within the world
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="componentSystem"></param>
        public void RemoveSystem<T>(T componentSystem) where T : ComponentSystem
        {
            if (!_componentSystems.Remove(componentSystem))
            {
                return; // return if the given component system wasn't found (already removed or give an invalid system)
            }

            componentSystem.RemoveFromWorld();
            Entities.UnsubscribeSystem(componentSystem);
        }

        internal void Dispose()
        {
            Entities.Dispose();
        }

        internal void Report()
        {
            ServerDomain.DeploymentProvider.ReportConnectionCount(Id, Connections.Count);
        }

        internal void Update()
        {
            _componentSystemsAlt.AddRange(_componentSystems);
            for (int i = 0; i < _componentSystemsAlt.Count; i++)
            {
                var system = _componentSystemsAlt[i];
                if (system.World != this) // in case the system was removed during an update
                {
                    continue;
                }

                system.Update();
            }
            _componentSystemsAlt.Clear();
        }
    }
}