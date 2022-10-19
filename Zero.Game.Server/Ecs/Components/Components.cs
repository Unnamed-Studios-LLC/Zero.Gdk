using System;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public unsafe ref struct Components
    {
        internal readonly EntityGroup Group;
        internal readonly byte* Chunk;
        internal readonly int Index;

        internal Components(EntityGroup group, byte* chunk, int index)
        {
            Group = group;
            Chunk = chunk;
            Index = index;
        }

        /// <summary>
        /// Returns a reference to component T found on the given entity id. This method will throw if the component doesn't exist.
        /// Consider using TryGetComponent you are unsure about the component's existence
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public ref T GetComponent<T>() where T : unmanaged
        {
            var type = TypeCache<T>.Type;
            if (Group == null)
            {
                throw new Exception($"Component type {typeof(T).FullName} not found. Use {nameof(TryGetComponent)} to avoid exceptions being thrown.");
            }

            if (!Group.Archetype.Contains(type)) // entity does not contain type
            {
                throw new Exception($"Component type {typeof(T).FullName} not found. Use {nameof(TryGetComponent)} to avoid exceptions being thrown.");
            }

            var componentListIndex = -1;
            for (int i = 0; i < Group.NonZeroComponentListCount; i++)
            {
                if (Group.NonZeroComponentTypes[i] == type)
                {
                    componentListIndex = i;
                    break;
                }
            }
            return ref Group.GetComponentRef<T>(Chunk, componentListIndex, Index);
        }

        /// <summary>
        /// Returns if the components contains a component of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public bool HasComponent<T>(uint entityId) where T : unmanaged
        {
            if (Group == null)
            {
                return false;
            }

            var type = TypeCache<T>.Type;
            return Group.Archetype.Contains(type);
        }


        /// <summary>
        /// Returns a reference to component T found in the components. Found is assigned to if the component exists.
        /// If found is false, the returned ref is a throwaway ref.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityId"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        public ref T TryGetComponent<T>(out bool found) where T : unmanaged
        {
            if (Group == null) // entity has no components
            {
                found = false;
                return ref TypeCache<T>.NullRef;
            }

            var type = TypeCache<T>.Type;
            if (!Group.Archetype.Contains(type)) // entity does not contain type
            {
                found = false;
                return ref TypeCache<T>.NullRef;
            }

            found = true;
            var componentListIndex = -1;
            for (int i = 0; i < Group.NonZeroComponentListCount; i++)
            {
                if (Group.NonZeroComponentTypes[i] == type)
                {
                    componentListIndex = i;
                    break;
                }
            }

            return ref Group.GetComponentRef<T>(Chunk, componentListIndex, Index);
        }
    }
}
