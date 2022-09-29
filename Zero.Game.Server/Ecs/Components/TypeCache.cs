using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zero.Game.Server
{
    internal delegate void AddDelegate(Entities entities, uint entityId, ref EntityReference reference);
    internal delegate void RemoveDelegate(Entities entities, uint entityId, in EntityReference reference);

    internal unsafe static class TypeCache
    {
        public static object Lock = new();
        public static int NextType = 1;
        public const int MaxComponentTypes = ushort.MaxValue;
        public static int DisabledType = 0;
        public static ulong DisabledArchetypeMask = 1;

        public static List<int> Sizes = new() { sizeof(Disabled) };
        public static List<bool> ZeroSize = new() { true };
        public static Dictionary<Type, int> Types = new() { [typeof(Disabled)] = DisabledType };
        public static List<AddDelegate> AddEventPublishers = new() { TypeCache<Disabled>.EventAdd };
        public static List<RemoveDelegate> RemoveEventPublishers = new() { TypeCache<Disabled>.EventRemove };

        static TypeCache()
        {

        }
    }

    internal static unsafe class TypeCache<T> where T : unmanaged
    {
        private static int? _index;

        public static T NullRef = default;
        public static int Type = Generate();
        public static bool ZeroSize;

        public static void EventAdd(Entities entities, uint entityId, ref EntityReference reference) => entities.PublishAddEvent(entityId, ref reference.GetComponent<T>(Type));
        public static void EventRemove(Entities entities, uint entityId, in EntityReference reference) => entities.PublishRemoveEvent(entityId, in reference.GetComponent<T>(Type));

        private static int Generate()
        {
            lock (TypeCache.Lock)
            {
                if (_index != null)
                {
                    return _index.Value;
                }

                if (TypeCache.NextType == TypeCache.MaxComponentTypes)
                {
                    throw new Exception("Maximum types reached");
                }

                if (sizeof(T) > ushort.MaxValue)
                {
                    throw new Exception($"Component {typeof(T).FullName} exceeds the max component size of {ushort.MaxValue}");
                }

                var type = typeof(T);
                if (type.Equals(typeof(Disabled)))
                {
                    _index = TypeCache.DisabledType;
                }
                else
                {
                    _index = TypeCache.NextType++;
                }
                ZeroSize = GenerateIsZeroSize(type);
                TypeCache.ZeroSize.Add(ZeroSize);
                TypeCache.Types.Add(type, _index.Value);
                TypeCache.Sizes.Add(sizeof(T));
                TypeCache.RemoveEventPublishers.Add(EventRemove);
                return _index.Value;
            }
        }

        private static bool GenerateIsZeroSize(Type type)
        {
            var zeroSize = type.IsValueType && !type.IsPrimitive &&
                type.GetFields((BindingFlags)0x34).All(fi => GenerateIsZeroSize(fi.FieldType));
            return zeroSize;
        }
    }
}
