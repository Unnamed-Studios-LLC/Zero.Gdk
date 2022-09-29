using Zero.Game.Mock.Components;
using Zero.Game.Mock.Data;
using Zero.Game.Server;
using Zero.Game.Shared;

namespace Zero.Game.Mock.Systems
{
    public class MockSystem : ComponentSystem, IAddEvent<PositionComponent>, IRemoveEvent<PositionComponent>, IAddEvent<TestComponent>
    {
        public void OnAdd(uint entityId, ref PositionComponent component)
        {
            Debug.Log("Added position to {0}", entityId);
        }

        public void OnAdd(uint entityId, ref TestComponent component)
        {
            Debug.Log("Added test to {0}", entityId);
        }

        public void OnRemove(uint entityId, in PositionComponent component)
        {
            Debug.Log("Removed position from {0}", entityId);
        }

        protected override void OnUpdate()
        {
            Entities.With<Tag1>().ForEach((uint entityId, ref PositionComponent position) =>
            {
                Entities.PushEvent(entityId, new TestData() { Value = entityId });
            });
        }
    }
}
