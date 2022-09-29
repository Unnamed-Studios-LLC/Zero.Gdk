using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public unsafe partial class Entities
    {
        private readonly List<EntityGroup> _groups = new();
        private readonly CompositeKeyDictionary<ulong, EntityGroup> _groupLocator = new();
        private readonly Dictionary<uint, EntityReference> _entityLocationMap = new();
        private readonly Dictionary<uint, EntityData> _entityData = new();
        private readonly ArrayCache<byte> _bufferCache = new(10, 100, 2);
        private readonly List<EntityData> _dataCache = new();
        private readonly List<IntPtr> _componentListIndicesCache = new();
        private readonly List<(EntityGroup, IntPtr, int)> _parallelChunkList = new();
        private int _usedListIndices = 0;
        private uint _nextEntityId = 1;
        private bool _iterating = false;

        internal void Dispose()
        {
            for (int i = 0; i < _groups.Count; i++)
            {
                _groups[i].Dispose();
            }
            _groups.Clear();

            foreach (var listIndices in _componentListIndicesCache)
            {
                Marshal.FreeHGlobal(listIndices);
            }
            _componentListIndicesCache.Clear();
        }

        internal bool TryGetEntityData(uint entityId, out EntityData entityData)
        {
            return _entityData.TryGetValue(entityId, out entityData);
        }

        private static void CopyOverlapingComponents(ref EntityReference source, ref EntityReference destination)
        {
            int j = -1;
            int destinationType;
            for (int i = 0; i < source.Group.NonZeroComponentListCount; i++)
            {
                var sourceType = source.Group.NonZeroComponentTypes[i];
                do
                {
                    if (++j >= destination.Group.NonZeroComponentListCount)
                    {
                        return;
                    }
                    destinationType = destination.Group.NonZeroComponentTypes[j];
                }
                while (destinationType != sourceType);

                var srcPtr = source.Group.GetComponent(source.ChunkIndex, i, source.ListIndex, out var size);
                var dstPtr = destination.Group.GetComponent(destination.ChunkIndex, j, destination.ListIndex, out size);
                Buffer.MemoryCopy(srcPtr, dstPtr, size, size);
            }
        }

        private unsafe void EnsureGroup(ulong* archetypes, int depth, out EntityGroup group)
        {
            if (_groupLocator.TryGetValue(archetypes, depth, out group))
            {
                return;
            }

            // construct archetype
            var archetypesArray = new ulong[depth];
            fixed (ulong* arrayPntr = archetypesArray)
            {
                for (int i = 0; i < depth; i++)
                {
                    *(arrayPntr + i) = *(archetypes + i);
                }
            }

            group = new EntityGroup(new EntityArchetype(archetypesArray));
            _groupLocator.Insert(archetypes, depth, group);
            _groups.Add(group);
        }

        private unsafe void ForEach<T>(T query) where T : IQuery
        {
            _iterating = true;

            try
            {
                int* indices = stackalloc int[6];
                query.AddRequiredArchetypes(ref _with, ref _withDepth);

                for (int i = 0; i < _groups.Count; i++)
                {
                    var group = _groups[i];
                    if (!GroupInQuery(group))
                    {
                        continue;
                    }

                    query.GetComponentListIndex(group, indices);

                    for (int j = 0; j < group.Chunks.Count; j++)
                    {
                        if (group.GetChunkCount(j) == 0)
                        {
                            continue;
                        }

                        query.Func(group, j, indices);
                    }
                }
            }
            finally
            {
                ZeroFilters();
                _iterating = false;
            }
        }

        private uint GenerateEntityId()
        {
            uint id;
            do
            {
                id = _nextEntityId++;
            }
            while (_entityLocationMap.ContainsKey(id));
            return id;
        }

        private EntityData GetEntityData()
        {
            EntityData data;
            if (_dataCache.Count > 0)
            {
                data = _dataCache[^1];
                _dataCache.RemoveAt(_dataCache.Count - 1);
            }
            else
            {
                data = new EntityData(100, _bufferCache);
            }
            return data;
        }

        private int* GetListIndices()
        {
            IntPtr ptr;
            if (_usedListIndices < _componentListIndicesCache.Count)
            {
                ptr = _componentListIndicesCache[_usedListIndices++];
                return (int*)ptr.ToPointer();
            }

            _usedListIndices++;
            ptr = Marshal.AllocHGlobal(sizeof(int) * 6);
            _componentListIndicesCache.Add(ptr);
            return (int*)ptr.ToPointer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GroupInQuery(EntityGroup group)
        {
            return group.Archetype.ContainsAny(_any, _anyDepth + 1) &&
                !group.Archetype.ContainsAny(_no, _noDepth + 1) &&
                group.Archetype.ContainsAll(_with, _withDepth + 1);
        }

        private unsafe void ParallelForEach<T>(T query) where T : IQuery
        {
            _iterating = true;
            try
            {
                _parallelChunkList.Clear();
                query.AddRequiredArchetypes(ref _with, ref _withDepth);

                for (int i = 0; i < _groups.Count; i++)
                {
                    var group = _groups[i];
                    if (!GroupInQuery(group))
                    {
                        continue;
                    }

                    var indices = GetListIndices();
                    query.GetComponentListIndex(group, indices);

                    for (int j = 0; j < group.Chunks.Count; j++)
                    {
                        if (group.GetChunkCount(j) == 0)
                        {
                            continue;
                        }

                        _parallelChunkList.Add((group, new IntPtr(indices), j));
                    }
                }

                Parallel.ForEach(_parallelChunkList, (pair) =>
                {
                    var (group, indicesPtr, chunkIndex) = pair;
                    var indices = (int*)indicesPtr.ToPointer();
                    query.Func(group, chunkIndex, indices);
                });

                ReturnListIndices();
            }
            finally
            {
                ZeroFilters();
                _iterating = false;
            }
        }

        private void ReturnEntityData(EntityData data)
        {
            data.Clear();
            _dataCache.Add(data);
        }

        private void ReturnListIndices()
        {
            _usedListIndices = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfIterating()
        {
            if (!_iterating)
            {
                return;
            }
            throw new Exception("Entity structural change is not allowed while iterating. Use a command buffer to execute changes after the iteration");
        }

        private void ZeroFilters()
        {
            _anyDepth = -1;
            _noDepth = 0;
            _withDepth = -1;
            _no.Archetypes[0] = TypeCache.DisabledArchetypeMask;
        }
    }
}
