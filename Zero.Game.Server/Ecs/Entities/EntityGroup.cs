using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zero.Game.Server
{
    internal unsafe class EntityGroup
    {
        private const int ChunkAllocSize = 16384;

        public EntityGroup(EntityArchetype archetype)
        {
            Archetype = archetype;

            NonZeroComponentTypes = archetype.GetNonZeroComponentTypes(out var nonZeroCount);
            NonZeroComponentListCount = nonZeroCount;

            ComponentTypes = archetype.GetComponentTypes(out var count);
            ComponentListCount = count;

            ComponentSizes = GetComponentSizes();
            ChunkCapacity = GetChunkCapacity();
            ComponentListOffsets = GetComponentListOffsets();
        }

        public EntityArchetype Archetype { get; }
        public List<IntPtr> Chunks { get; } = new();
        public int ChunkCapacity { get; }
        public int ComponentListCount { get; }
        public int* ComponentListOffsets { get; }
        public int* ComponentSizes { get; }
        public int* ComponentTypes { get; }
        public int NonZeroComponentListCount { get; }
        public int* NonZeroComponentTypes { get; }

        /// <summary>
        /// Frees unmanaged memory
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < Chunks.Count; i++)
            {
                Marshal.FreeHGlobal(Chunks[i]);
            }
            Chunks.Clear();

            if (ComponentListOffsets != null)
            {
                Marshal.FreeHGlobal(new IntPtr(ComponentListOffsets));
            }
            if (ComponentSizes != null)
            {
                Marshal.FreeHGlobal(new IntPtr(ComponentSizes));
            }
            if (ComponentTypes != null)
            {
                Marshal.FreeHGlobal(new IntPtr(ComponentTypes));
            }
            if (NonZeroComponentTypes != null)
            {
                Marshal.FreeHGlobal(new IntPtr(NonZeroComponentTypes));
            }
        }

        /// <summary>
        /// Returns the first available entity slot
        /// </summary>
        /// <returns></returns>
        public EntityReference GetNextSlot(uint entityId)
        {
            int chunkIndex = -1;
            EntityChunkHeader* chunk = null;
            for (int i = 0; i < Chunks.Count; i++)
            {
                chunk = (EntityChunkHeader*)Chunks[i].ToPointer();
                if (chunk->Count < ChunkCapacity)
                {
                    chunkIndex = i;
                    break;
                }
            }

            if (chunkIndex == -1)
            {
                var chunkPtr = CreateChunk();
                chunk = (EntityChunkHeader*)chunkPtr.ToPointer();
                chunkIndex = Chunks.Count;
                Chunks.Add(chunkPtr);
            }

            var listIndex = chunk->Count++;
            var entityIds = (uint*)((byte*)chunk + sizeof(EntityChunkHeader));
            *(entityIds + listIndex) = entityId; // set entity id
            return new EntityReference(this, chunkIndex, listIndex);
        }

        /// <summary>
        /// Returns a pointer to a component in the given list
        /// </summary>
        /// <param name="chunkIndex"></param>
        /// <param name="componentListIndex"></param>
        /// <param name="listIndex"></param>
        /// <param name="elementSize"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* GetComponent(int chunkIndex, int componentListIndex, int listIndex, out int elementSize)
        {
            if (componentListIndex == -1)
            {
                elementSize = 0;
                return null;
            }
            elementSize = ComponentSizes[componentListIndex];
            return (byte*)Chunks[chunkIndex].ToPointer() + ComponentListOffsets[componentListIndex] + listIndex * elementSize;
        }

        /// <summary>
        /// Returns a reference to a component in the given list
        /// </summary>
        /// <param name="chunkIndex"></param>
        /// <param name="componentListIndex"></param>
        /// <param name="listIndex"></param>
        /// <param name="elementSize"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponentRef<T>(int chunkIndex, int componentListIndex, int listIndex) where T : unmanaged
        {
            var list = GetList<T>(chunkIndex, componentListIndex);
            if (list == null)
            {
                return ref TypeCache<T>.NullRef;
            }
            return ref *(list + listIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponentRef<T>(byte* chunk, int componentListIndex, int listIndex) where T : unmanaged
        {
            var list = GetList<T>(chunk, componentListIndex);
            if (list == null)
            {
                return ref TypeCache<T>.NullRef;
            }
            return ref *(list + listIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte* GetChunk(int chunkIndex)
        {
            return (byte*)Chunks[chunkIndex].ToPointer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetChunkCount(int chunkIndex)
        {
            return ((EntityChunkHeader*)Chunks[chunkIndex].ToPointer())->Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint* GetEntityIdList(int chunkIndex)
        {
            return (uint*)((byte*)Chunks[chunkIndex].ToPointer() + sizeof(EntityChunkHeader));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetList<T>(int chunkIndex, int componentListIndex) where T : unmanaged
        {
            return GetList<T>((byte*)Chunks[chunkIndex].ToPointer(), componentListIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetList<T>(byte* chunk, int componentListIndex) where T : unmanaged
        {
            if (componentListIndex == -1)
            {
                return null;
            }
            return (T*)(chunk + ComponentListOffsets[componentListIndex]);
        }

        /// <summary>
        /// Removes a component at the given index and returns an entityId that was remapped to the given index (0 if no remapping)
        /// </summary>
        /// <param name="listIndex"></param>
        /// <param name="remappedEntityId"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe uint Remove(int chunkIndex, int listIndex)
        {
            var chunk = (byte*)Chunks[chunkIndex].ToPointer();
            var lastIndex = --((EntityChunkHeader*)chunk)->Count;
            if (listIndex == lastIndex)
            {
                // last entity was removed
                return 0;
            }

            // patch hole by remapping the last entity
            var listPntr = chunk + sizeof(EntityChunkHeader);
            var entityId = *((uint*)listPntr + listIndex) = *((uint*)listPntr + lastIndex); // move entity id
            listPntr += sizeof(uint) * ChunkCapacity;
            for (int i = 0; i < NonZeroComponentListCount; i++)
            {
                var elementSize = ComponentSizes[i];
                Buffer.MemoryCopy(listPntr + lastIndex * elementSize, listPntr + listIndex * elementSize, elementSize, elementSize);
                listPntr += elementSize * ChunkCapacity;
            }

            // return entity id that was remapped
            return entityId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IntPtr CreateChunk()
        {
            var ptr = Marshal.AllocHGlobal(ChunkAllocSize);
            var chunk = (EntityChunkHeader*)ptr;
            chunk->Count = 0;
            return ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int* GetComponentListOffsets()
        {
            var offsets = (int*)Marshal.AllocHGlobal(sizeof(int) * NonZeroComponentListCount).ToPointer();
            var offset = sizeof(EntityChunkHeader) + sizeof(uint) * ChunkCapacity; // ignore header + entity ids
            for (int i = 0; i < NonZeroComponentListCount; i++)
            {
                offsets[i] = offset;
                offset += ComponentSizes[i] * ChunkCapacity;
            }
            return offsets;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int* GetComponentSizes()
        {
            var sizes = (int*)Marshal.AllocHGlobal(sizeof(int) * NonZeroComponentListCount).ToPointer();
            var pntr = sizes;
            for (int i = 0; i < NonZeroComponentListCount; i++)
            {
                sizes[i] = TypeCache.Sizes[NonZeroComponentTypes[i]];
            }
            return sizes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetChunkCapacity()
        {
            var headerSize = sizeof(EntityChunkHeader);
            var lineSize = GetLineSize();
            var lines = (ChunkAllocSize - headerSize) / lineSize;
            return lines;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetLineSize()
        {
            var lineSize = sizeof(uint); // entityId
            for (int i = 0; i < NonZeroComponentListCount; i++)
            {
                lineSize += ComponentSizes[i];
            }
            return lineSize;
        }

        #region List Index Getters

        public void GetComponentListIndex<T0>(int* results)
            where T0 : unmanaged
        {
            var type0 = TypeCache<T0>.Type;
            results[0] = -1;
            for (int i = 0; i < NonZeroComponentListCount; i++)
            {
                var listType = NonZeroComponentTypes[i];
                if (type0 == listType)
                {
                    results[0] = i;
                }
            }
        }

        public void GetComponentListIndex<T0, T1>(int* results)
            where T0 : unmanaged
            where T1 : unmanaged
        {
            var type0 = TypeCache<T0>.Type;
            var type1 = TypeCache<T1>.Type;
            ((long*)results)[0] = -1L | (-1L << 32); // set both in one op
            for (int i = 0; i < NonZeroComponentListCount; i++)
            {
                var listType = NonZeroComponentTypes[i];
                if (type0 == listType)
                {
                    results[0] = i;
                }
                else if (type1 == listType)
                {
                    results[1] = i;
                }
            }
        }

        public void GetComponentListIndex<T0, T1, T2>(int* results)
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
        {
            var type0 = TypeCache<T0>.Type;
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            ((long*)results)[0] = -1L | (-1L << 32); // set both in one op
            results[2] = -1;
            for (int i = 0; i < NonZeroComponentListCount; i++)
            {
                var listType = NonZeroComponentTypes[i];
                if (type0 == listType)
                {
                    results[0] = i;
                }
                else if (type1 == listType)
                {
                    results[1] = i;
                }
                else if (type2 == listType)
                {
                    results[2] = i;
                }
            }
        }

        public void GetComponentListIndex<T0, T1, T2, T3>(int* results)
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
        {
            var type0 = TypeCache<T0>.Type;
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            ((long*)results)[0] = -1L | (-1L << 32); // set both in one op
            ((long*)results)[1] = -1L | (-1L << 32);
            for (int i = 0; i < NonZeroComponentListCount; i++)
            {
                var listType = NonZeroComponentTypes[i];
                if (type0 == listType)
                {
                    results[0] = i;
                }
                else if (type1 == listType)
                {
                    results[1] = i;
                }
                else if (type2 == listType)
                {
                    results[2] = i;
                }
                else if (type3 == listType)
                {
                    results[3] = i;
                }
            }
        }

        public void GetComponentListIndex<T0, T1, T2, T3, T4>(int* results)
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
        {
            var type0 = TypeCache<T0>.Type;
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            var type4 = TypeCache<T4>.Type;
            ((long*)results)[0] = -1L | (-1L << 32); // set both in one op
            ((long*)results)[1] = -1L | (-1L << 32); // set both in one op
            results[4] = -1;
            for (int i = 0; i < NonZeroComponentListCount; i++)
            {
                var listType = NonZeroComponentTypes[i];
                if (type0 == listType)
                {
                    results[0] = i;
                }
                else if (type1 == listType)
                {
                    results[1] = i;
                }
                else if (type2 == listType)
                {
                    results[2] = i;
                }
                else if (type3 == listType)
                {
                    results[3] = i;
                }
                else if (type4 == listType)
                {
                    results[4] = i;
                }
            }
        }

        public void GetComponentListIndex<T0, T1, T2, T3, T4, T5>(int* results)
            where T0 : unmanaged
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
        {
            var type0 = TypeCache<T0>.Type;
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            var type4 = TypeCache<T4>.Type;
            var type5 = TypeCache<T5>.Type;
            ((long*)results)[0] = -1L | (-1L << 32); // set both in one op
            ((long*)results)[1] = -1L | (-1L << 32); // set both in one op
            ((long*)results)[2] = -1L | (-1L << 32); // set both in one op
            for (int i = 0; i < NonZeroComponentListCount; i++)
            {
                var listType = NonZeroComponentTypes[i];
                if (type0 == listType)
                {
                    results[0] = i;
                }
                else if (type1 == listType)
                {
                    results[1] = i;
                }
                else if (type2 == listType)
                {
                    results[2] = i;
                }
                else if (type3 == listType)
                {
                    results[3] = i;
                }
                else if (type4 == listType)
                {
                    results[4] = i;
                }
                else if (type5 == listType)
                {
                    results[5] = i;
                }
            }
        }

        #endregion
    }
}