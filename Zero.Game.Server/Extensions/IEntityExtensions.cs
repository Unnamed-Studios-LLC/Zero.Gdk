namespace Zero.Game.Server
{
    public static class IEntityExtensions
    {
        /// <summary>
        /// Adds a new component to the current entity. If the component already exists, the values are overriden.
        /// This operation invokes a structural change and cannot be called within queries or from threads other than the main thread.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityId"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        public static ref T AddComponent<T>(this IEntity entity, T component = default) where T : unmanaged
        {
            return ref entity.Entities.AddComponent<T>(entity.EntityId, component);
        }

        /// <summary>
        /// Gets a component of type T for the current entity id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ref T GetComponent<T>(this IEntity entity) where T : unmanaged
        {
            return ref entity.Entities.GetComponent<T>(entity.EntityId);
        }

        /// <summary>
        /// Returns a reference struct to an entities components.
        /// Used for multiple component gets and checks.
        /// This method will throw if the entity doesn't exist.
        /// The returned Components are invalid after any structural change
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public static EntityComponents GetComponents(this IEntity entity)
        {
            return entity.Entities.GetComponents(entity.EntityId);
        }

        /// <summary>
        /// Returns if the current entity id has a given component of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool HasComponent<T>(this IEntity entity) where T : unmanaged
        {
            return entity.Entities.HasComponent<T>(entity.EntityId);
        }

        /// <summary>
        /// Returns if the current entity is marked as disabled
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public static bool IsDisabled(this IEntity entity)
        {
            return entity.Entities.IsDisabled(entity.EntityId);
        }

        /// <summary>
        /// Removes a component T from a the current entity. If the component doesn't exist on the entity, no operation is executed.
        /// An exception is thrown if the given entity does not exist.
        /// This operation invokes a structural change and cannot be called within queries or from threads other than the main thread.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityId"></param>
        public static void RemoveComponent<T>(this IEntity entity) where T : unmanaged
        {
            entity.Entities.RemoveComponent<T>(entity.EntityId);
        }

        /// <summary>
        /// Sets the current entity's disabled value. By default, disabled entities are not included in queries.
        /// This operation invokes a structural change and cannot be called within queries or from threads other than the main thread.
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="disabled"></param>
        public static void SetDisabled(this IEntity entity, bool disabled)
        {
            entity.Entities.SetDisabled(entity.EntityId, disabled);
        }

        /// <summary>
        /// Trys to get a component of type T for the current entity id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ref T TryGetComponent<T>(this IEntity entity, out bool found) where T : unmanaged
        {
            return ref entity.Entities.TryGetComponent<T>(entity.EntityId, out found);
        }
    }
}
