using System;
using System.Reflection;

namespace Zero.Game.Server
{
    internal abstract class ComponentDefinition
    {
        public ComponentDefinition(ushort type, int priority, Type componentType)
        {
            Type = type;
            Priority = priority;
            DetermineOverrides(componentType);
        }

        public bool OverridesUpdate { get; set; }
        public bool OverridesViewUpdate { get; set; }
        public int Priority { get; set; }
        public ushort Type { get; set; }


        public abstract ComponentSystem CreateSystem(uint worldId);

        private void DetermineOverrides(Type componentType)
        {
            var baseType = typeof(Component);
            OverridesUpdate = componentType.GetMethod("OnUpdate", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType != baseType;
            OverridesViewUpdate = componentType.GetMethod("OnViewUpdate", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType != baseType;
        }
    }

    internal class ComponentDefinition<T> : ComponentDefinition
        where T : Component
    {
        public ComponentDefinition(ushort type, int priority) : base(type, priority, typeof(T))
        {
        }

        public override ComponentSystem CreateSystem(uint worldId)
        {
            return new ComponentSystem<T>(Type, worldId, OverridesUpdate, OverridesViewUpdate);
        }
    }
}
