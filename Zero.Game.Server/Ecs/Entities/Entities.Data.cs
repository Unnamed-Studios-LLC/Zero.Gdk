using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public sealed unsafe partial class Entities
    {
        /// <summary>
        /// Gets the persistent data set for a given entity. If no data has been set, default T is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public T GetPersistent<T>(uint entityId) where T : unmanaged
        {
            ThrowHelper.ThrowIfDataNotDefined<T>();
            if (!_entityData.TryGetValue(entityId, out var entityData))
            {
                ThrowHelper.ThrowInvalidEntityId();
            }

            return entityData.GetPersistent<T>();
        }

        /// <summary>
        /// Pushes event data for an entity. Event data is pushed once to containing views
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public void PushEvent<T>(uint entityId, T data) where T : unmanaged
        {
            ThrowHelper.ThrowIfDataNotDefined<T>();
            if (!_entityData.TryGetValue(entityId, out var entityData))
            {
                ThrowHelper.ThrowInvalidEntityId();
            }

            entityData.PushEvent(Time.Total, &data);
        }

        /// <summary>
        /// Pushes persistent data for an entity. Persistent data is stored as part of the entity and pushed to views when they first are "aware" of an entity.
        /// Persistent data changes are also pushed as events to any views that are already "aware" of the entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public void PushPersistent<T>(uint entityId, T data) where T : unmanaged
        {
            ThrowHelper.ThrowIfDataNotDefined<T>();
            if (!_entityData.TryGetValue(entityId, out var entityData))
            {
                ThrowHelper.ThrowInvalidEntityId();
            }

            entityData.PushPersistent(Time.Total, &data);
        }
        /// <summary>
        /// Trys to get the persistent data set for a given entity. If no data has been set, false is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public bool TryGetPersistent<T>(uint entityId, out T data) where T : unmanaged
        {
            ThrowHelper.ThrowIfDataNotDefined<T>();
            if (!_entityData.TryGetValue(entityId, out var entityData))
            {
                ThrowHelper.ThrowInvalidEntityId();
            }

            return entityData.TryGetPersistent(out data);
        }
    }
}
