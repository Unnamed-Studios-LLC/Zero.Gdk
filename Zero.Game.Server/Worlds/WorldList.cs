using System.Collections.Generic;
using System.Linq;

namespace Zero.Game.Server
{
    internal class WorldList
    {
        private readonly Dictionary<uint, World> _worldMap = new();
        private readonly List<World> _worlds = new(100);
        private readonly List<World> _parallelWorlds = new(100);

        public void Add(World world)
        {
            if (world.Parallel)
            {
                _parallelWorlds.Add(world);
            }
            else
            {
                _worlds.Add(world);
            }
            _worldMap[world.Id] = world;
        }

        public World[] GetAllWorlds()
        {
            return _worlds.Concat(_parallelWorlds).ToArray();
        }


        public List<World> GetParallelWorldsList()
        {
            return _parallelWorlds;
        }

        public List<World> GetWorldsList()
        {
            return _worlds;
        }

        public bool TryGet(uint worldId, out World world)
        {
            return _worldMap.TryGetValue(worldId, out world);
        }

        public bool TryRemove(uint worldId, out World world)
        {
            if (!_worldMap.TryGetValue(worldId, out world))
            {
                return false;
            }
            _worldMap.Remove(worldId);
            if (world.Parallel)
            {
                _parallelWorlds.Remove(world);
            }
            else
            {
                _worlds.Remove(world);
            }
            return true;
        }
    }
}
