using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zero.Game.Server
{
    internal unsafe struct EntityArchetype
    {
        public EntityArchetype(ulong[] archetypes)
        {
            Archetypes = archetypes;
        }

        public ulong[] Archetypes { get; }
        public int DepthCount => Archetypes.Length;
        public bool IsDisabledArchetype => (Archetypes[0] & TypeCache.DisabledArchetypeMask) == TypeCache.DisabledArchetypeMask;
        public int NonZeroTypeCount => GetNonZeroTypeCount();
        public int TypeCount => GetTypeCount();

        public static bool operator ==(EntityArchetype left, EntityArchetype right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityArchetype left, EntityArchetype right)
        {
            return !(left == right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddType(ulong* archetypes, int type)
        {
            archetypes[type / 64] |= 1ul << (type % 64);
        }

        public bool Contains(int type)
        {
            var depth = type / 64;
            if (depth >= Archetypes.Length)
            {
                return false;
            }
            var relType = 1u << (type % 64);
            return (Archetypes[depth] & relType) == relType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ContainsAll(EntityArchetype other)
        {
            return ContainsAll(other, other.DepthCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ContainsAll(EntityArchetype other, int depth)
        {
            fixed (ulong* otherArchPntr = other.Archetypes)
            {
                return ContainsAll(otherArchPntr, depth);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ContainsAll(ulong* other, int otherDepth)
        {
            if (otherDepth > DepthCount)
            {
                return false;
            }

            fixed (ulong* curArchPntr = Archetypes)
            {
                for (int i = 0; i < otherDepth; i++)
                {
                    if ((*(curArchPntr + i) & *(other + i)) != *(other + i))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ContainsAny(EntityArchetype other)
        {
            return ContainsAny(other, other.DepthCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ContainsAny(EntityArchetype other, int depth)
        {
            fixed (ulong* otherArchPntr = other.Archetypes)
            {
                return ContainsAny(otherArchPntr, depth);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool ContainsAny(ulong* other, int otherDepth)
        {
            var minDepth = DepthCount;
            if (otherDepth < minDepth)
            {
                minDepth = otherDepth;
            }

            if (minDepth <= 0)
            {
                return true;
            }

            fixed (ulong* curArchPntr = Archetypes)
            {
                for (int i = 0; i < minDepth; i++)
                {
                    if ((*(curArchPntr + i) & *(other + i)) != 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Equals(EntityArchetype other)
        {
            if (Archetypes.Length != other.Archetypes.Length)
            {
                return false;
            }

            for (int i = 0; i < Archetypes.Length && i < other.Archetypes.Length; i++)
            {
                if (Archetypes[i] != other.Archetypes[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int* GetComponentTypes(out int count)
        {
            count = TypeCount;
            var types = (int*)Marshal.AllocHGlobal(sizeof(int) * count).ToPointer();

            var pntr = types;
            for (int i = 0; i < Archetypes.Length; i++)
            {
                var relArchetype = Archetypes[i];
                for (int j = 0; j < 64; j++)
                {
                    var relType = 1ul << j;
                    if (relType > relArchetype)
                    {
                        break;
                    }

                    if ((relArchetype & relType) == relType)
                    {
                        *pntr = i * 64 + j;
                        pntr++;
                    }
                }
            }

            return types;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetComponentTypes(int* types, int count)
        {
            var pntr = types;
            for (int i = 0; i < Archetypes.Length; i++)
            {
                var relArchetype = Archetypes[i];
                for (int j = 0; j < 64; j++)
                {
                    var relType = 1ul << j;
                    if (relType > relArchetype)
                    {
                        break;
                    }

                    if ((relArchetype & relType) == relType)
                    {
                        *pntr = i * 64 + j;
                        pntr++;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int* GetNonZeroComponentTypes(out int count)
        {
            count = NonZeroTypeCount;
            var types = (int*)Marshal.AllocHGlobal(sizeof(int) * count).ToPointer();

            var pntr = types;
            for (int i = 0; i < Archetypes.Length; i++)
            {
                var relArchetype = Archetypes[i];
                for (int j = 0; j < 64; j++)
                {
                    var relType = 1ul << j;
                    if (relType > relArchetype)
                    {
                        break;
                    }

                    if ((relArchetype & relType) == relType &&
                        !TypeCache.ZeroSize[j])
                    {
                        *pntr = i * 64 + j;
                        pntr++;
                    }
                }
            }

            return types;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNonZeroTypeCount()
        {
            int count = 0;
            for (int i = 0; i < Archetypes.Length; i++)
            {
                var relArchetype = Archetypes[i];
                for (int j = 0; j < 64; j++)
                {
                    var relType = 1ul << j;
                    if (relType > relArchetype)
                    {
                        break;
                    }

                    if ((relArchetype & relType) == relType &&
                        !TypeCache.ZeroSize[j])
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetTypeCount()
        {
            int count = 0;
            for (int i = 0; i < Archetypes.Length; i++)
            {
                var relArchetype = Archetypes[i];
                for (int j = 0; j < 64; j++)
                {
                    var relType = 1ul << j;
                    if (relType > relArchetype)
                    {
                        break;
                    }

                    if ((relArchetype & relType) == relType)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public override int GetHashCode()
        {
            int hash = (int)Archetypes[0];
            for (int i = 1; i < Archetypes.Length; i++)
            {
                hash = (hash << 5) + hash ^ (int)Archetypes[i];
            }
            return hash;
        }
    }
}
