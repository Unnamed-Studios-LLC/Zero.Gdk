using System.Collections.Generic;

namespace Zero.Game.Server
{
    public interface IComponentContainer
    {
        void AddComponent(Component component);
        Component GetComponent(ushort type);
        T GetComponent<T>()
            where T : Component;
        IEnumerable<Component> GetComponents();
        IEnumerable<T> GetComponents<T>()
            where T : Component;
        void GetComponents<T>(List<T> list);
        void RemoveComponent(Component component);
    }
}
