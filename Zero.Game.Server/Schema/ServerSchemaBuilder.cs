using System;
using System.Collections.Generic;
using System.Linq;
using Zero.Game.Common;
using Zero.Game.Common.Schema;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public class ServerSchemaBuilder : CommonSchemaBuilder
    {
        private readonly Dictionary<ushort, ComponentDefinition> _componentDefinitions = new Dictionary<ushort, ComponentDefinition>();

        public ServerSchemaBuilder Component<T>(Action<ComponentBuilder<T>> buildAction = null)
            where T : Component, new()
        {
            var type = new T().Type;

            if (_componentDefinitions.ContainsKey(type))
            {
                throw new InvalidOperationException($"Invalid Component type defined for {typeof(T).FullName}. Type {type} has already been defined");
            }

            var builder = new ComponentBuilder<T>(type);
            buildAction?.Invoke(builder);

            var definition = builder.Build();
            _componentDefinitions.Add(type, definition);

            return this;
        }

        internal ServerSchema Build()
        {
            return new ServerSchema(_componentDefinitions.Values.ToList(),
                GetDataDefinitions());
        }
    }
}
