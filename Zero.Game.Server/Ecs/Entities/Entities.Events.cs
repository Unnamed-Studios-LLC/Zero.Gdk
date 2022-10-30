using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public sealed partial class Entities
    {
        private static class SystemSubscriptionCache<T>
        {
            static SystemSubscriptionCache()
            {
                (AddEventTypes, RemoveEventTypes) = GetImplementedEventTypes();
            }

            public static int[] AddEventTypes;
            public static int[] RemoveEventTypes;

            private static (int[], int[]) GetImplementedEventTypes()
            {
                int addCount = 0;
                int removeCount = 0;

                var type = typeof(T);
                var addGenericType = typeof(IAddEvent<>);
                var removeGenericType = typeof(IRemoveEvent<>);

                var interfaces = type.GetInterfaces();
                foreach (var @interface in interfaces)
                {
                    if (!@interface.IsConstructedGenericType)
                    {
                        continue;
                    }

                    if (@interface.GetGenericTypeDefinition() == addGenericType)
                    {
                        addCount++;
                    }
                    else if (@interface.GetGenericTypeDefinition() == removeGenericType)
                    {
                        removeCount++;
                    }
                }

                var addTypes = new int[addCount];
                var removeTypes = new int[removeCount];

                addCount = 0;
                removeCount = 0;

                foreach (var @interface in interfaces)
                {
                    if (@interface.GetGenericTypeDefinition() == addGenericType)
                    {
                        RuntimeHelpers.RunClassConstructor(@interface.TypeHandle);
                        addTypes[addCount++] = TypeCache.Types[@interface.GenericTypeArguments[0]];
                    }
                    else if (@interface.GetGenericTypeDefinition() == removeGenericType)
                    {
                        RuntimeHelpers.RunClassConstructor(@interface.TypeHandle);
                        removeTypes[removeCount++] = TypeCache.Types[@interface.GenericTypeArguments[0]];
                    }
                }

                return (addTypes, removeTypes);
            }
        }

        private bool _inEvent = false;

        private readonly Dictionary<int, List<object>> _addEvents = new();
        private readonly Dictionary<int, List<object>> _removeEvents = new();

        internal bool PublishAddEvent<T>(uint entityId, ref T component) where T : unmanaged
        {
            if (!_addEvents.TryGetValue(TypeCache<T>.Type, out var eventList) ||
                eventList.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < eventList.Count; i++)
            {
                try
                {
                    _inEvent = true;
                    ((IAddEvent<T>)eventList[i]).OnAdd(entityId, ref component);
                }
                catch (Exception e)
                {
                    Debug.LogError(e, "An error occured during {0}", nameof(IAddEvent<T>.OnAdd));
                }
                finally
                {
                    _inEvent = false;
                }
            }
            return true;
        }

        internal bool PublishRemoveEvent<T>(uint entityId, in T component) where T : unmanaged
        {
            if (!_removeEvents.TryGetValue(TypeCache<T>.Type, out var eventList) ||
                eventList.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < eventList.Count; i++)
            {
                try
                {
                    _inEvent = true;
                    ((IRemoveEvent<T>)eventList[i]).OnRemove(entityId, in component);
                }
                catch (Exception e)
                {
                    Debug.LogError(e, "An error occured during {0}", nameof(IRemoveEvent<T>.OnRemove));
                }
                finally
                {
                    _inEvent = false;
                }
            }

            return true;
        }

        internal void SubscribeSystem<T>(T componentSystem) where T : ComponentSystem
        {
            AddEvents(_addEvents, SystemSubscriptionCache<T>.AddEventTypes, componentSystem);
            AddEvents(_removeEvents, SystemSubscriptionCache<T>.RemoveEventTypes, componentSystem);
        }

        internal void UnsubscribeSystem<T>(T componentSystem) where T : ComponentSystem
        {
            RemoveEvents(_addEvents, SystemSubscriptionCache<T>.AddEventTypes, componentSystem);
            RemoveEvents(_removeEvents, SystemSubscriptionCache<T>.RemoveEventTypes, componentSystem);
        }

        private static void AddEvents(Dictionary<int, List<object>> eventMap, int[] eventTypes, object eventHandler)
        {
            for (int i = 0; i < eventTypes.Length; i++)
            {
                var type = eventTypes[i];
                if (!eventMap.TryGetValue(type, out var list))
                {
                    list = new List<object>();
                    eventMap.Add(type, list);
                }
                list.Add(eventHandler);
            }
        }

        private static void RemoveEvents(Dictionary<int, List<object>> eventMap, int[] eventTypes, object eventHandler)
        {
            for (int i = 0; i < eventTypes.Length; i++)
            {
                var type = eventTypes[i];
                if (!eventMap.TryGetValue(type, out var list))
                {
                    continue; // should never happen, since add is always called first
                }
                list.Remove(eventHandler);
            }
        }
    }
}
