using System.Collections.Generic;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public interface IComponentContainer
    {
        void AddComponent(Component component);
        T GetComponent<T>()
            where T : Component;

        IData GetData(ushort type);
        T GetData<T>()
            where T : IData;

        IEnumerable<Component> GetComponents();
        IEnumerable<T> GetComponents<T>()
            where T : Component;
        void GetComponents<T>(List<T> list);
        void RemoveComponent(Component component);
    }
}
