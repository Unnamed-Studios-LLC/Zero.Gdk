using System;
using System.Diagnostics;
using Zero.Game.Shared;
using Debug = Zero.Game.Shared.Debug;

namespace Zero.Game.Server
{
    public abstract class ComponentSystem
    {
        private bool _started;

        /// <summary>
        /// Shared command buffer for the world
        /// </summary>
        public CommandBuffer Commands { get; private set; }

        /// <summary>
        /// Access to world entities
        /// </summary>
        public Entities Entities { get; private set; }

        /// <summary>
        /// The duration of Update last update
        /// </summary>
        public long LastUpdateDuration { get; internal set; }

        /// <summary>
        /// The world being executed in
        /// </summary>
        public World World { get; private set; }

        internal void AddToWorld(World world)
        {
            World = world;
            Entities = world.Entities;
            Commands = world.Commands;
        }

        internal void RemoveFromWorld()
        {
            World = null;
            Entities = null;
            Commands = null;
        }

        internal void Remove()
        {
            try
            {
                OnRemove();
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(OnRemove));
            }
        }

        internal void Update()
        {
            var t = Stopwatch.GetTimestamp();
            if (!_started)
            {
                _started = true;
                try
                {
                    OnStart();
                }
                catch (Exception e)
                {
                    Debug.LogError(e, "An error occurred during {0}", nameof(OnStart));
                }
            }

            try
            {
                OnUpdate();
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(OnUpdate));
            }

            Commands.Execute();
            LastUpdateDuration = Stopwatch.GetTimestamp() - t;
        }

        protected void AddSystem<T>(T system) where T : ComponentSystem => World.AddSystem(system);
        protected T GetSystem<T>() where T : ComponentSystem => World.GetSystem<T>();
        protected bool RemoveSystem<T>() where T : ComponentSystem => World.RemoveSystem<T>();
        protected bool RemoveSystem<T>(T system) where T : ComponentSystem => World.RemoveSystem(system);

        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnRemove() { }
    }
}
