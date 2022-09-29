using System.Threading.Tasks;
using Zero.Game.Mock.Components;
using Zero.Game.Mock.Data;
using Zero.Game.Mock.Systems;
using Zero.Game.Server;
using Zero.Game.Shared;

namespace Zero.Game.Mock
{
    public class MockPlugin : ServerPlugin
    {
        private readonly Task<bool> s_completedSyncTrue = Task.FromResult(true);
        private readonly Task<bool> s_completedSyncFalse = Task.FromResult(false);

        public override void BuildData(DataBuilder builder)
        {
            builder.Define<TestData>();
        }

        public override async Task StartDeploymentAsync()
        {
            await Deployment.StartWorldAsync(new StartWorldRequest
            {
                WorldId = 1
            });
        }

        public override Task<bool> LoadConnectionAsync(Connection connection)
        {
            connection.MessageHandler = new ServerMessageHandler();

            return s_completedSyncTrue;
        }

        public override Task<bool> LoadWorldAsync(World world)
        {
            var layout = new EntityLayout()
                .Define<PositionComponent>()
                .Define<TestComponent>();

            world.AddSystem(new MockSystem());
            world.Entities.ApplyLayout(world.EntityId, layout);

            var entityId = world.Entities.CreateEntity();
            world.Entities.AddComponent<Tag1>(entityId);
            world.Entities.AddComponent<PositionComponent>(entityId);

            entityId = world.Entities.CreateEntity();
            world.Entities.AddComponent<Tag2>(entityId);
            world.Entities.AddComponent<PositionComponent>(entityId);

            entityId = world.Entities.CreateEntity();
            world.Entities.AddComponent<Tag3>(entityId);
            world.Entities.AddComponent<PositionComponent>(entityId);

            entityId = world.Entities.CreateEntity();
            world.Entities.AddComponent<Tag4>(entityId);
            world.Entities.AddComponent<PositionComponent>(entityId);

            entityId = world.Entities.CreateEntity();
            world.Entities.AddComponent<Tag5>(entityId);
            world.Entities.AddComponent<PositionComponent>(entityId);

            return s_completedSyncTrue;
        }
    }
}
