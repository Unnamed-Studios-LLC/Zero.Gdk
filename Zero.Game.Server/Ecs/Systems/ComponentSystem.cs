using System;
using Zero.Game.Shared;

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

        internal void Update()
        {
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
        }

        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }
    }
}
