using System;
using System.Collections.Generic;

namespace Zero.Game.Server
{
    internal abstract class ComponentSystem
    {
        public ComponentSystem(Type type, uint worldId)
        {
            Type = type;
            WorldId = worldId;
        }

        public Type Type { get; }
        protected uint WorldId { get; }

        public abstract void Clear();

        /*
        public abstract void Deregister(Component component);
        */

        public abstract void Register(Component component);

        public abstract void Update();

        public abstract void ViewUpdate();
    }

    internal class ComponentSystem<T> : ComponentSystem
        where T : Component
    {
        private List<T> _components = new();
        private List<T> _componentsAlt = new();

        private readonly bool _overridesUpdate;
        private readonly bool _overridesViewUpdate;

        public ComponentSystem(Type type, uint worldId, bool overridesUpdate, bool overridesViewUpdate) : base(type, worldId)
        {
            _overridesUpdate = overridesUpdate;
            _overridesViewUpdate = overridesViewUpdate;
        }

        public override void Clear()
        {
            if (!_overridesUpdate && !_overridesViewUpdate)
            {
                return;
            }

            _componentsAlt.Clear();
        }

        /*
        public override void Deregister(Component component)
        {
            _components.Remove(component as T);
        }
        */

        public override void Register(Component component)
        {
            if (!_overridesUpdate && !_overridesViewUpdate)
            {
                return;
            }

            _componentsAlt.Add(component as T);
        }

        public override void Update()
        {
            if (!_overridesUpdate && !_overridesViewUpdate)
            {
                return;
            }

            var temp = _components;
            temp.Clear();

            _components = _componentsAlt;
            _componentsAlt = temp;

            for (int i = 0; i < _components.Count; i++)
            {
                var c = _components[i];
                if (c.RemovedFromWorld || c.RemovedFromEntity)
                {
                    if (c.RemovedFromEntity)
                    {
                        c.RemoveFromEntity();
                    }

                    if (c.RegisteredWorldId == WorldId)
                    {
                        c.RegisteredWorldId = 0;
                    }
                    continue;
                }
                _componentsAlt.Add(c);

                if (!c.Active || !_overridesUpdate)
                {
                    continue;
                }
                _components[i].Update();
            }
        }

        public override void ViewUpdate()
        {
            if (!_overridesViewUpdate)
            {
                return;
            }

            for (int i = 0; i < _components.Count; i++)
            {
                var c = _components[i];
                if (!c.Active)
                {
                    continue;
                }
                _components[i].ViewUpdate();
            }
        }
    }
}
