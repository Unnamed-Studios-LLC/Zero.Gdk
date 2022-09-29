using System;
using System.Runtime.CompilerServices;
using Zero.Game.Shared;

#pragma warning disable CS0168 // Variable is declared but never used

namespace Zero.Game.Server
{
    internal unsafe struct EntityQuery : IQuery
    {
        private readonly EntityQueryFunc _func;

        public EntityQuery(EntityQueryFunc func)
        {
            _func = func;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AddRequiredArchetypes(ref EntityArchetype archetype, ref int maxDepth)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Func(EntityGroup group, int chunkIndex, int* indices)
        {
            var entityIdList = group.GetEntityIdList(chunkIndex);
            var count = group.GetChunkCount(chunkIndex);
            for (int k = 0; k < count; k++)
            {
                try
                {
                    _func.Invoke(
                        *entityIdList
                    );
                }
                catch (Exception e)
                {
#if DEBUG
                    throw;
#else
                    Debug.LogError(e, "An error occurred during entity query");
#endif
                }
                entityIdList++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void GetComponentListIndex(EntityGroup group, int* indices)
        {

        }
    }

    internal unsafe struct EntityQuery<T1> : IQuery where T1 : unmanaged
    {
        private readonly int _type1;
        private readonly EntityQueryFunc<T1> _func;

        public EntityQuery(EntityQueryFunc<T1> func)
        {
            _type1 = TypeCache<T1>.Type;
            _func = func;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AddRequiredArchetypes(ref EntityArchetype archetype, ref int maxDepth)
        {
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Func(EntityGroup group, int chunkIndex, int* indices)
        {
            var entityIdList = group.GetEntityIdList(chunkIndex);
            var list = group.GetList<T1>(chunkIndex, indices[0]);
            var count = group.GetChunkCount(chunkIndex);
            for (int k = 0; k < count; k++)
            {
                try
                {
                    _func.Invoke(
                        *entityIdList,
                        ref Unsafe.AsRef<T1>(list)
                    );
                }
                catch (Exception e)
                {
#if DEBUG
                    throw;
#else
                    Debug.LogError(e, "An error occurred during entity query");
#endif
                }
                entityIdList++;
                list++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void GetComponentListIndex(EntityGroup group, int* indices)
        {
            group.GetComponentListIndex<T1>(indices);
        }
    }

    internal unsafe struct EntityQuery<T1, T2> : IQuery
        where T1 : unmanaged
        where T2 : unmanaged
    {
        private readonly int _type1;
        private readonly int _type2;
        private readonly EntityQueryFunc<T1, T2> _func;

        public EntityQuery(EntityQueryFunc<T1, T2> func)
        {
            _type1 = TypeCache<T1>.Type;
            _type2 = TypeCache<T2>.Type;
            _func = func;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AddRequiredArchetypes(ref EntityArchetype archetype, ref int maxDepth)
        {
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type1);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Func(EntityGroup group, int chunkIndex, int* indices)
        {
            var entityIdList = group.GetEntityIdList(chunkIndex);
            var list1 = group.GetList<T1>(chunkIndex, indices[0]);
            var list2 = group.GetList<T2>(chunkIndex, indices[1]);
            var count = group.GetChunkCount(chunkIndex);
            for (int k = 0; k < count; k++)
            {
                try
                {
                    _func.Invoke(
                        *entityIdList,
                        ref Unsafe.AsRef<T1>(list1),
                        ref Unsafe.AsRef<T2>(list2)
                    );
                }
                catch (Exception e)
                {
#if DEBUG
                    throw;
#else
                    Debug.LogError(e, "An error occurred during entity query");
#endif
                }
                entityIdList++;
                list1++;
                list2++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void GetComponentListIndex(EntityGroup group, int* indices)
        {
            group.GetComponentListIndex<T1, T2>(indices);
        }
    }

    internal unsafe struct EntityQuery<T1, T2, T3> : IQuery
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        private readonly int _type1;
        private readonly int _type2;
        private readonly int _type3;
        private readonly EntityQueryFunc<T1, T2, T3> _func;

        public EntityQuery(EntityQueryFunc<T1, T2, T3> func)
        {
            _type1 = TypeCache<T1>.Type;
            _type2 = TypeCache<T2>.Type;
            _type3 = TypeCache<T3>.Type;
            _func = func;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AddRequiredArchetypes(ref EntityArchetype archetype, ref int maxDepth)
        {
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type1);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type2);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Func(EntityGroup group, int chunkIndex, int* indices)
        {
            var entityIdList = group.GetEntityIdList(chunkIndex);
            var list1 = group.GetList<T1>(chunkIndex, indices[0]);
            var list2 = group.GetList<T2>(chunkIndex, indices[1]);
            var list3 = group.GetList<T3>(chunkIndex, indices[2]);
            var count = group.GetChunkCount(chunkIndex);
            for (int k = 0; k < count; k++)
            {
                try
                {
                    _func.Invoke(
                        *entityIdList,
                        ref Unsafe.AsRef<T1>(list1),
                        ref Unsafe.AsRef<T2>(list2),
                        ref Unsafe.AsRef<T3>(list3)
                    );
                }
                catch (Exception e)
                {
#if DEBUG
                    throw;
#else
                    Debug.LogError(e, "An error occurred during entity query");
#endif
                }
                entityIdList++;
                list1++;
                list2++;
                list3++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void GetComponentListIndex(EntityGroup group, int* indices)
        {
            group.GetComponentListIndex<T1, T2, T3>(indices);
        }
    }

    internal unsafe struct EntityQuery<T1, T2, T3, T4> : IQuery
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
    {
        private readonly int _type1;
        private readonly int _type2;
        private readonly int _type3;
        private readonly int _type4;
        private readonly EntityQueryFunc<T1, T2, T3, T4> _func;

        public EntityQuery(EntityQueryFunc<T1, T2, T3, T4> func)
        {
            _type1 = TypeCache<T1>.Type;
            _type2 = TypeCache<T2>.Type;
            _type3 = TypeCache<T3>.Type;
            _type4 = TypeCache<T4>.Type;
            _func = func;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AddRequiredArchetypes(ref EntityArchetype archetype, ref int maxDepth)
        {
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type1);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type2);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type3);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Func(EntityGroup group, int chunkIndex, int* indices)
        {
            var entityIdList = group.GetEntityIdList(chunkIndex);
            var list1 = group.GetList<T1>(chunkIndex, indices[0]);
            var list2 = group.GetList<T2>(chunkIndex, indices[1]);
            var list3 = group.GetList<T3>(chunkIndex, indices[2]);
            var list4 = group.GetList<T4>(chunkIndex, indices[3]);
            var count = group.GetChunkCount(chunkIndex);
            for (int k = 0; k < count; k++)
            {
                try
                {
                    _func.Invoke(
                        *entityIdList,
                        ref Unsafe.AsRef<T1>(list1),
                        ref Unsafe.AsRef<T2>(list2),
                        ref Unsafe.AsRef<T3>(list3),
                        ref Unsafe.AsRef<T4>(list4)
                    );
                }
                catch (Exception e)
                {
#if DEBUG
                    throw;
#else
                    Debug.LogError(e, "An error occurred during entity query");
#endif
                }
                entityIdList++;
                list1++;
                list2++;
                list3++;
                list4++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void GetComponentListIndex(EntityGroup group, int* indices)
        {
            group.GetComponentListIndex<T1, T2, T3, T4>(indices);
        }
    }

    internal unsafe struct EntityQuery<T1, T2, T3, T4, T5> : IQuery
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
    {
        private readonly int _type1;
        private readonly int _type2;
        private readonly int _type3;
        private readonly int _type4;
        private readonly int _type5;
        private readonly EntityQueryFunc<T1, T2, T3, T4, T5> _func;

        public EntityQuery(EntityQueryFunc<T1, T2, T3, T4, T5> func)
        {
            _type1 = TypeCache<T1>.Type;
            _type2 = TypeCache<T2>.Type;
            _type3 = TypeCache<T3>.Type;
            _type4 = TypeCache<T4>.Type;
            _type5 = TypeCache<T5>.Type;
            _func = func;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AddRequiredArchetypes(ref EntityArchetype archetype, ref int maxDepth)
        {
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type1);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type2);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type3);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type4);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type5);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Func(EntityGroup group, int chunkIndex, int* indices)
        {
            var entityIdList = group.GetEntityIdList(chunkIndex);
            var list1 = group.GetList<T1>(chunkIndex, indices[0]);
            var list2 = group.GetList<T2>(chunkIndex, indices[1]);
            var list3 = group.GetList<T3>(chunkIndex, indices[2]);
            var list4 = group.GetList<T4>(chunkIndex, indices[3]);
            var list5 = group.GetList<T5>(chunkIndex, indices[4]);
            var count = group.GetChunkCount(chunkIndex);
            for (int k = 0; k < count; k++)
            {
                try
                {
                    _func.Invoke(
                        *entityIdList,
                        ref Unsafe.AsRef<T1>(list1),
                        ref Unsafe.AsRef<T2>(list2),
                        ref Unsafe.AsRef<T3>(list3),
                        ref Unsafe.AsRef<T4>(list4),
                        ref Unsafe.AsRef<T5>(list5)
                    );
                }
                catch (Exception e)
                {
#if DEBUG
                    throw;
#else
                    Debug.LogError(e, "An error occurred during entity query");
#endif
                }
                entityIdList++;
                list1++;
                list2++;
                list3++;
                list4++;
                list5++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void GetComponentListIndex(EntityGroup group, int* indices)
        {
            group.GetComponentListIndex<T1, T2, T3, T4, T5>(indices);
        }
    }

    internal unsafe struct EntityQuery<T1, T2, T3, T4, T5, T6> : IQuery
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
    {
        private readonly int _type1;
        private readonly int _type2;
        private readonly int _type3;
        private readonly int _type4;
        private readonly int _type5;
        private readonly int _type6;
        private readonly EntityQueryFunc<T1, T2, T3, T4, T5, T6> _func;

        public EntityQuery(EntityQueryFunc<T1, T2, T3, T4, T5, T6> func)
        {
            _type1 = TypeCache<T1>.Type;
            _type2 = TypeCache<T2>.Type;
            _type3 = TypeCache<T3>.Type;
            _type4 = TypeCache<T4>.Type;
            _type5 = TypeCache<T5>.Type;
            _type6 = TypeCache<T6>.Type;
            _func = func;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void AddRequiredArchetypes(ref EntityArchetype archetype, ref int maxDepth)
        {
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type1);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type2);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type3);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type4);
            Entities.AddtoFilter(ref archetype, ref maxDepth, _type5);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Func(EntityGroup group, int chunkIndex, int* indices)
        {
            var entityIdList = group.GetEntityIdList(chunkIndex);
            var list1 = group.GetList<T1>(chunkIndex, indices[0]);
            var list2 = group.GetList<T2>(chunkIndex, indices[1]);
            var list3 = group.GetList<T3>(chunkIndex, indices[2]);
            var list4 = group.GetList<T4>(chunkIndex, indices[3]);
            var list5 = group.GetList<T5>(chunkIndex, indices[4]);
            var list6 = group.GetList<T6>(chunkIndex, indices[5]);
            var count = group.GetChunkCount(chunkIndex);
            for (int k = 0; k < count; k++)
            {
                try
                {
                    _func.Invoke(
                        *entityIdList,
                        ref Unsafe.AsRef<T1>(list1),
                        ref Unsafe.AsRef<T2>(list2),
                        ref Unsafe.AsRef<T3>(list3),
                        ref Unsafe.AsRef<T4>(list4),
                        ref Unsafe.AsRef<T5>(list5),
                        ref Unsafe.AsRef<T6>(list6)
                    );
                }
                catch (Exception e)
                {
#if DEBUG
                    throw;
#else
                    Debug.LogError(e, "An error occurred during entity query");
#endif
                }
                entityIdList++;
                list1++;
                list2++;
                list3++;
                list4++;
                list5++;
                list6++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void GetComponentListIndex(EntityGroup group, int* indices)
        {
            group.GetComponentListIndex<T1, T2, T3, T4, T5, T6>(indices);
        }
    }
}

#pragma warning restore CS0168 // Variable is declared but never used