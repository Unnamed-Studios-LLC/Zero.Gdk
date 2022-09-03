using System;
using System.Collections.Generic;
using System.Linq;

namespace Zero.Game.Server
{
    internal class ComponentSystemCollection
    {
        private readonly List<ComponentSystem> _componentSystems;
        private readonly Dictionary<Type, ComponentSystem> _componentSystemsMap;

        public ComponentSystemCollection(List<ComponentSystem> componentSystems)
        {
            _componentSystems = componentSystems;
            _componentSystemsMap = componentSystems.ToDictionary(x => x.Type);
        }

        public void Clear()
        {
            for (int i = 0; i < _componentSystems.Count; i++)
            {
                _componentSystems[i].Clear();
            }
        }

        /*
        public void Deregister(Component component)
        {
            if (!_componentSystemsMap.TryGetValue(component.Type, out var system))
            {
                Debug.LogError("Component {0} has not been defined in the Schema", component.GetType());
                return;
            }

            system.Deregister(component);
        }
        */

        public void Register(Component component)
        {
            var type = component.GetType();
            if (!_componentSystemsMap.TryGetValue(type, out var system))
            {
                Debug.LogError("Component {0} has not been defined in the Schema", type);
                return;
            }

            system.Register(component);
        }

        public void UpdateAll()
        {
            for (int i = 0; i < _componentSystems.Count; i++)
            {
                _componentSystems[i].Update();
            }
        }

        public void ViewUpdateAll()
        {
            for (int i = 0; i < _componentSystems.Count; i++)
            {
                _componentSystems[i].ViewUpdate();
            }
        }
    }
}
