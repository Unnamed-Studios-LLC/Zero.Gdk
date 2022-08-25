using System.Collections.Generic;
using System.Threading.Tasks;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public class Entity : DataContainer, IComponentContainer
    {
        private readonly List<Component> _components = new();

        public bool Active { get; private set; } = true;
        public World World { get; private set; }

        internal bool InternalActive { get; set; } = true;
        internal bool IsActive => Active && InternalActive && World != null;
        internal override ObjectType ObjectType => ObjectType.Entity;

        public void AddComponent(Component component)
        {
            _components.Add(component);

            component.AddToEntity(this);

            if (World != null)
            {
                component.AddToWorld();
            }
        }

        public Component GetComponent(ushort type)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                var component = _components[i];
                if (component.Type == type)
                {
                    return component;
                }
            }
            return null;
        }

        public T GetComponent<T>()
            where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                var component = _components[i];
                if (component is T tComponent)
                {
                    return tComponent;
                }
            }
            return null;
        }

        public IEnumerable<Component> GetComponents()
        {
            return _components;
        }

        public IEnumerable<T> GetComponents<T>()
            where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                var component = _components[i];
                if (component is T tComponent)
                {
                    yield return tComponent;
                }
            }
        }

        public void GetComponents<T>(List<T> list)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                var component = _components[i];
                if (component is T tComponent)
                {
                    list.Add(tComponent);
                }
            }
        }

        public void RemoveComponent(Component component)
        {
            component.RemovedFromEntity = true;
        }

        public void SetActive(bool active)
        {
            Active = active;
        }

        internal void AddToWorld(World world)
        {
            World = world;

            for (int i = 0; i < _components.Count; i++)
            {
                var component = _components[i];
                component.AddToWorld();
            }
        }

        internal async Task DestroyAsync()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                var component = _components[i];
                if (component is not IAsyncComponent asyncComponent)
                {
                    continue;
                }

                await asyncComponent.DestroyAsync()
                    .ConfigureAwait(false);
            }
        }

        internal async Task<bool> InitAsync()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                var component = _components[i];
                if (component is not IAsyncComponent asyncComponent)
                {
                    continue;
                }

                if (!await asyncComponent.InitAsync()
                        .ConfigureAwait(false))
                {
                    return false;
                }
            }
            return true;
        }

        internal void RemoveComponentInternal(Component component)
        {
            _components.Remove(component);
        }

        internal void RemoveFromWorld()
        {
            if (World == null)
            {
                return;
            }

            for (int i = 0; i < _components.Count; i++)
            {
                var component = _components[i];
                component.RemoveFromWorld();
            }

            World = null;
        }
    }
}
