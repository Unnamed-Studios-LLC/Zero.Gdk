using System;

namespace Zero.Game.Server
{
    public sealed unsafe partial class Entities
    {
        private EntityArchetype _any = new(new ulong[1]);
        private EntityArchetype _no = new(new ulong[1] { TypeCache.DisabledArchetypeMask });
        private EntityArchetype _with = new(new ulong[1]);
        private int _anyDepth = -1;
        private int _noDepth = 0;
        private int _withDepth = -1;

        public Entities Any<T1>()
            where T1 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            AddtoFilter(ref _any, ref _anyDepth, type1);
            return this;
        }

        public Entities Any<T1, T2>()
            where T1 : unmanaged
            where T2 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            AddtoFilter(ref _any, ref _anyDepth, type1);
            AddtoFilter(ref _any, ref _anyDepth, type2);
            return this;
        }

        public Entities Any<T1, T2, T3>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            AddtoFilter(ref _any, ref _anyDepth, type1);
            AddtoFilter(ref _any, ref _anyDepth, type2);
            AddtoFilter(ref _any, ref _anyDepth, type3);
            return this;
        }

        public Entities Any<T1, T2, T3, T4>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            var type4 = TypeCache<T4>.Type;
            AddtoFilter(ref _any, ref _anyDepth, type1);
            AddtoFilter(ref _any, ref _anyDepth, type2);
            AddtoFilter(ref _any, ref _anyDepth, type3);
            AddtoFilter(ref _any, ref _anyDepth, type4);
            return this;
        }

        public Entities Any<T1, T2, T3, T4, T5>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            var type4 = TypeCache<T4>.Type;
            var type5 = TypeCache<T5>.Type;
            AddtoFilter(ref _any, ref _anyDepth, type1);
            AddtoFilter(ref _any, ref _anyDepth, type2);
            AddtoFilter(ref _any, ref _anyDepth, type3);
            AddtoFilter(ref _any, ref _anyDepth, type4);
            AddtoFilter(ref _any, ref _anyDepth, type5);
            return this;
        }

        public Entities Any<T1, T2, T3, T4, T5, T6>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
            where T6 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            var type4 = TypeCache<T4>.Type;
            var type5 = TypeCache<T5>.Type;
            var type6 = TypeCache<T6>.Type;
            AddtoFilter(ref _any, ref _anyDepth, type1);
            AddtoFilter(ref _any, ref _anyDepth, type2);
            AddtoFilter(ref _any, ref _anyDepth, type3);
            AddtoFilter(ref _any, ref _anyDepth, type4);
            AddtoFilter(ref _any, ref _anyDepth, type5);
            AddtoFilter(ref _any, ref _anyDepth, type6);
            return this;
        }

        public Entities IncludeDisabled()
        {
            _no.Archetypes[0] &= ~TypeCache.DisabledArchetypeMask;
            return this;
        }

        public Entities No<T1>()
            where T1 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            AddtoFilter(ref _no, ref _noDepth, type1);
            return this;
        }

        public Entities No<T1, T2>()
            where T1 : unmanaged
            where T2 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            AddtoFilter(ref _no, ref _noDepth, type1);
            AddtoFilter(ref _no, ref _noDepth, type2);
            return this;
        }

        public Entities No<T1, T2, T3>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            AddtoFilter(ref _no, ref _noDepth, type1);
            AddtoFilter(ref _no, ref _noDepth, type2);
            AddtoFilter(ref _no, ref _noDepth, type3);
            return this;
        }

        public Entities No<T1, T2, T3, T4>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            var type4 = TypeCache<T4>.Type;
            AddtoFilter(ref _no, ref _noDepth, type1);
            AddtoFilter(ref _no, ref _noDepth, type2);
            AddtoFilter(ref _no, ref _noDepth, type3);
            AddtoFilter(ref _no, ref _noDepth, type4);
            return this;
        }

        public Entities No<T1, T2, T3, T4, T5>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            var type4 = TypeCache<T4>.Type;
            var type5 = TypeCache<T5>.Type;
            AddtoFilter(ref _no, ref _noDepth, type1);
            AddtoFilter(ref _no, ref _noDepth, type2);
            AddtoFilter(ref _no, ref _noDepth, type3);
            AddtoFilter(ref _no, ref _noDepth, type4);
            AddtoFilter(ref _no, ref _noDepth, type5);
            return this;
        }

        public Entities No<T1, T2, T3, T4, T5, T6>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
            where T6 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            var type4 = TypeCache<T4>.Type;
            var type5 = TypeCache<T5>.Type;
            var type6 = TypeCache<T6>.Type;
            AddtoFilter(ref _no, ref _noDepth, type1);
            AddtoFilter(ref _no, ref _noDepth, type2);
            AddtoFilter(ref _no, ref _noDepth, type3);
            AddtoFilter(ref _no, ref _noDepth, type4);
            AddtoFilter(ref _no, ref _noDepth, type5);
            AddtoFilter(ref _no, ref _noDepth, type6);
            return this;
        }

        public Entities With<T1>()
            where T1 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            AddtoFilter(ref _with, ref _withDepth, type1);
            return this;
        }

        public Entities With<T1, T2>()
            where T1 : unmanaged
            where T2 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            AddtoFilter(ref _with, ref _withDepth, type1);
            AddtoFilter(ref _with, ref _withDepth, type2);
            return this;
        }

        public Entities With<T1, T2, T3>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            AddtoFilter(ref _with, ref _withDepth, type1);
            AddtoFilter(ref _with, ref _withDepth, type2);
            AddtoFilter(ref _with, ref _withDepth, type3);
            return this;
        }

        public Entities With<T1, T2, T3, T4>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            var type4 = TypeCache<T4>.Type;
            AddtoFilter(ref _with, ref _withDepth, type1);
            AddtoFilter(ref _with, ref _withDepth, type2);
            AddtoFilter(ref _with, ref _withDepth, type3);
            AddtoFilter(ref _with, ref _withDepth, type4);
            return this;
        }

        public Entities With<T1, T2, T3, T4, T5>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            var type4 = TypeCache<T4>.Type;
            var type5 = TypeCache<T5>.Type;
            AddtoFilter(ref _with, ref _withDepth, type1);
            AddtoFilter(ref _with, ref _withDepth, type2);
            AddtoFilter(ref _with, ref _withDepth, type3);
            AddtoFilter(ref _with, ref _withDepth, type4);
            AddtoFilter(ref _with, ref _withDepth, type5);
            return this;
        }

        public Entities With<T1, T2, T3, T4, T5, T6>()
            where T1 : unmanaged
            where T2 : unmanaged
            where T3 : unmanaged
            where T4 : unmanaged
            where T5 : unmanaged
            where T6 : unmanaged
        {
            var type1 = TypeCache<T1>.Type;
            var type2 = TypeCache<T2>.Type;
            var type3 = TypeCache<T3>.Type;
            var type4 = TypeCache<T4>.Type;
            var type5 = TypeCache<T5>.Type;
            var type6 = TypeCache<T6>.Type;
            AddtoFilter(ref _with, ref _withDepth, type1);
            AddtoFilter(ref _with, ref _withDepth, type2);
            AddtoFilter(ref _with, ref _withDepth, type3);
            AddtoFilter(ref _with, ref _withDepth, type4);
            AddtoFilter(ref _with, ref _withDepth, type5);
            AddtoFilter(ref _with, ref _withDepth, type6);
            return this;
        }

        internal static void AddtoFilter(ref EntityArchetype archetype, ref int maxDepth, int type)
        {
            var depth = type / 64;
            if (depth > maxDepth)
            {
                for (int i = maxDepth; i < depth; i++)
                {
                    archetype.Archetypes[i + 1] = 0;
                }
                maxDepth = depth;
            }

            if (depth >= archetype.Archetypes.Length)
            {
                var newArchetypes = new ulong[depth + 1];
                fixed (ulong* src = archetype.Archetypes)
                fixed (ulong* dst = newArchetypes)
                {
                    Buffer.MemoryCopy(src, dst, archetype.Archetypes.Length * sizeof(ulong), archetype.Archetypes.Length * sizeof(ulong));
                }

                archetype = new EntityArchetype(newArchetypes);
            }

            archetype.Archetypes[depth] |= 1ul << (type % 64);
        }

        internal static void AddtoFilter(ulong* archetype, ref int maxDepth, int type)
        {
            var depth = type / 64;
            if (depth > maxDepth)
            {
                for (int i = maxDepth; i < depth; i++)
                {
                    archetype[i + 1] = 0;
                }
                maxDepth = depth;
            }

            archetype[depth] |= 1ul << (type % 64);
        }
    }
}
