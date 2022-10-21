using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Zero.Game.Server
{
    public unsafe sealed class EntityLayout
    {
        private abstract class EntityLayoutComponent
        {
            public abstract int Type { get; }

            public abstract void PublishAdd(uint entityId, byte* list, int listIndex, Entities entities);
            public abstract void Set(byte* list, int listIndex);
        }

        private class EntityLayoutComponent<T> : EntityLayoutComponent where T : unmanaged
        {
            public T Default { get; set; }
            public override int Type => TypeCache<T>.Type;

            public override void PublishAdd(uint entityId, byte* list, int listIndex, Entities entities)
            {
                ref var @ref = ref Unsafe.AsRef<T>((T*)list + listIndex);
                entities.PublishAddEvent(entityId, ref @ref);;
            }

            public override void Set(byte* list, int listIndex)
            {
                *((T*)list + listIndex) = Default;
            }
        }

        private readonly SortedList<int, EntityLayoutComponent> _components = new();
        private readonly Dictionary<int, EntityLayoutComponent> _componentMap = new();

        internal EntityArchetype Archetype { get; private set; } = new EntityArchetype(Array.Empty<ulong>());

        public unsafe EntityLayout Define<T>(T @default = default) where T : unmanaged
        {
            var type = TypeCache<T>.Type;
            if (_componentMap.TryGetValue(type, out var component))
            {
                ((EntityLayoutComponent<T>)component).Default = @default;
                return this;
            }

            var typedComponent = new EntityLayoutComponent<T>()
            {
                Default = @default
            };

            var typeDepth = type / 64;
            if (typeDepth >= Archetype.DepthCount)
            {
                var newArchetypes = new ulong[typeDepth + 1];
                fixed (ulong* newArchPntr = newArchetypes)
                fixed (ulong* curArchPntr = Archetype.Archetypes)
                {
                    for (int i = 0; i < Archetype.DepthCount; i++)
                    {
                        *(newArchPntr + i) = *(curArchPntr + i);
                    }
                }
                Archetype = new EntityArchetype(newArchetypes);
            }
            Archetype.Archetypes[typeDepth] |= 1ul << (type % 64);
            
            _components.Add(TypeCache<T>.Type, typedComponent);
            _componentMap.Add(type, typedComponent);

            return this;
        }

        internal void Set(uint entityId, ref EntityReference reference, EntityArchetype previousArchetype, Entities entities)
        {
            int j = 0;
            EntityLayoutComponent component = null;
            byte* chunk = (byte*)reference.Group.Chunks[reference.ChunkIndex].ToPointer();
            for (int i = 0; i < reference.Group.NonZeroComponentListCount; i++)
            {
                var sourceType = reference.Group.NonZeroComponentTypes[i];
                var list = chunk + reference.Group.ComponentListOffsets[i];
                if (component == null || component.Type < sourceType)
                {
                    do
                    {
                        if (j >= _components.Count)
                        {
                            return;
                        }
                        component = _components.Values[j++];
                    }
                    while (component.Type < sourceType);
                }

                if (component.Type == sourceType)
                {
                    var isAdded = previousArchetype.Archetypes != null && !previousArchetype.Contains(sourceType);
                    component.Set(list, reference.ListIndex);
                }
            }

            component = null;
            j = 0;
            for (int i = 0; i < reference.Group.ComponentListCount; i++)
            {
                var sourceType = reference.Group.ComponentTypes[i];
                var list = chunk + reference.Group.ComponentListOffsets[i];
                if (component == null || component.Type < sourceType)
                {
                    do
                    {
                        if (j >= _components.Count)
                        {
                            return;
                        }
                        component = _components.Values[j++];
                    }
                    while (component.Type < sourceType);
                }

                if (component.Type == sourceType)
                {
                    var isAdded = previousArchetype.Archetypes == null || !previousArchetype.Contains(sourceType);
                    if (!isAdded)
                    {
                        continue;
                    }

                    component.PublishAdd(entityId, list, reference.ListIndex, entities);
                }
            }
        }
    }
}
