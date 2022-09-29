using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Zero.Game.Server
{
    internal class WorldList
    {
        private readonly Dictionary<uint, World> _worldMap = new();
        private readonly List<World> _worlds = new(100);

        public void Add(World world)
        {
            _worlds.Add(world);
            _worldMap[world.Id] = world;
        }

        public Span<World> GetWorlds()
        {
            return CollectionsMarshal.AsSpan(_worlds);
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
            _worlds.Remove(world);
            return true;
        }
    }
}
