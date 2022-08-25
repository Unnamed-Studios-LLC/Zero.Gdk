using System;
using System.Collections.Generic;
using System.Linq;
using Zero.Game.Common;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    internal class ServerSchema : CommonSchema
    {
        private readonly List<ComponentDefinition> _sortedComponentDefinitions;
        private readonly Dictionary<ushort, ComponentDefinition> _componentDefinitions;

        public ServerSchema(List<ComponentDefinition> componentDefinitions,
            List<DataDefinition> dataDefinitions) : base(dataDefinitions)
        {
            _sortedComponentDefinitions = componentDefinitions.OrderBy(x => x.Priority)
                .ToList();

            _componentDefinitions = componentDefinitions.ToDictionary(x => x.Type);
        }

        public ComponentSystemCollection CreateComponentSystemCollection(uint worldId)
        {
            var systems = _sortedComponentDefinitions.Select(x => x.CreateSystem(worldId))
                .ToList();

            return new ComponentSystemCollection(systems);
        }

        public ComponentDefinition GetComponentDefinition(ushort type)
        {
            if (!_componentDefinitions.TryGetValue(type, out var definition))
            {
                return null;
            }
            return definition;
        }
    }
}
