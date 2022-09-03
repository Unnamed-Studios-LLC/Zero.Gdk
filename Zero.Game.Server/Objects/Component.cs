using System;
using System.Collections.Generic;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public abstract class Component : IComponentContainer
    {
        public Connection Connection => Entity as Connection;
        public Entity Entity { get; private set; }
        public World World => Entity?.World;

        internal bool Active => !RemovedFromEntity && !RemovedFromWorld && (Entity?.IsActive ?? false);
        internal uint RegisteredWorldId { get; set; }
        internal bool RemovedFromEntity { get; set; }
        internal bool RemovedFromWorld { get; set; }

        public void AddComponent(Component component)
        {
            Entity.AddComponent(component);
        }

        public T GetComponent<T>()
            where T : Component
        {
            return Entity.GetComponent<T>();
        }

        public IEnumerable<Component> GetComponents()
        {
            return Entity.GetComponents();
        }

        public IEnumerable<T> GetComponents<T>() where T : Component
        {
            return Entity.GetComponents<T>();
        }

        public void GetComponents<T>(List<T> list)
        {
            Entity.GetComponents(list);
        }

        public IData GetData(ushort type)
        {
            return Entity?.GetData(type);
        }

        public T GetData<T>() where T : IData
        {
            if (Entity == null)
            {
                return default;
            }
            return Entity.GetData<T>();
        }

        public void PushPrivate(IData data)
        {
            Entity?.PushPrivate(data);
        }

        public void PushPublic(IData data)
        {
            Entity?.PushPublic(data);
        }

        public void RemoveComponent(Component component)
        {
            Entity.RemoveComponent(component);
        }

        protected virtual void OnAdd()
        {

        }

        protected virtual void OnAddToWorld()
        {

        }

        protected virtual void OnRemove()
        {

        }

        protected virtual void OnRemoveFromWorld()
        {

        }

        protected virtual void OnUpdate()
        {

        }

        protected virtual void OnViewUpdate()
        {

        }

        internal void AddToEntity(Entity entity)
        {
            Entity = entity;

            try
            {
                OnAdd();
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnAdd));
            }
        }

        internal void AddToWorld()
        {
            RemovedFromWorld = false;
            World.RegisterComponent(this);
            try
            {
                OnAddToWorld();
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnAddToWorld));
            }
        }

        internal void RemoveFromEntity()
        {
            try
            {
                OnRemove();
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnRemove));
            }

            Entity.RemoveComponentInternal(this);
            Entity = null;
        }

        internal void RemoveFromWorld()
        {
            try
            {
                OnRemoveFromWorld();
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnRemoveFromWorld));
            }
            RemovedFromWorld = true;
        }

        internal void Update()
        {
            try
            {
                OnUpdate();
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnUpdate));
            }
        }

        internal void ViewUpdate()
        {
            try
            {
                OnViewUpdate();
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnViewUpdate));
            }
        }
    }
}