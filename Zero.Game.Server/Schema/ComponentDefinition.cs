using System;
using System.Reflection;

namespace Zero.Game.Server
{
    internal abstract class ComponentDefinition
    {
        private static Type s_baseType = typeof(Component);

        public ComponentDefinition(Type type, int priority)
        {
            Type = type;
            Priority = priority;
            OverridesUpdate = type.GetMethod("OnUpdate", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType != s_baseType;
            OverridesViewUpdate = type.GetMethod("OnViewUpdate", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType != s_baseType;
        }

        public Type Type { get; }
        public int Priority { get; }
        public bool OverridesUpdate { get; }
        public bool OverridesViewUpdate { get; }

        public abstract ComponentSystem CreateSystem(uint worldId);
    }

    internal class ComponentDefinition<T> : ComponentDefinition
        where T : Component
    {
        public ComponentDefinition(int priority) : base(typeof(T), priority)
        {
        }

        public override ComponentSystem CreateSystem(uint worldId)
        {
            return new ComponentSystem<T>(Type, worldId, OverridesUpdate, OverridesViewUpdate);
        }
    }
}
