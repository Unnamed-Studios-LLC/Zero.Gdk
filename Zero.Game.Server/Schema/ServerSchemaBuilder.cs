using System;
using System.Collections.Generic;
using System.Linq;
using Zero.Game.Common.Schema;

namespace Zero.Game.Server
{
    public class ServerSchemaBuilder : CommonSchemaBuilder
    {
        private readonly Dictionary<Type, ComponentDefinition> _componentDefinitions = new();

        public ServerSchemaBuilder Component<T>(Action<ComponentBuilder<T>> buildAction = null)
            where T : Component, new()
        {
            var type = typeof(T);
            if (_componentDefinitions.ContainsKey(type))
            {
                throw new InvalidOperationException($"Type {type.FullName} has already been defined");
            }

            var builder = new ComponentBuilder<T>();
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
