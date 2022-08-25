using System;
using System.Collections.Generic;
using System.Linq;
using Zero.Game.Shared;

namespace Zero.Game.Common.Schema
{
    public abstract class CommonSchemaBuilder : ISchemaBuilder
    {
        private readonly Dictionary<ushort, DataDefinition> _dataDefinitions = new Dictionary<ushort, DataDefinition>();

        public ISchemaBuilder Data<T>(Action<IDataBuilder<T>> buildAction = null) where T : IData, new()
        {
            var model = new T();
            var type = model.Type;

            if (_dataDefinitions.ContainsKey(type))
            {
                throw new InvalidOperationException($"Invalid Data type defined for {typeof(T).FullName}. Type {type} has already been defined for {_dataDefinitions[type].ClassType.FullName}");
            }

            var builder = new DataBuilder<T>(type);
            buildAction?.Invoke(builder);

            var definition = builder.Build();
            _dataDefinitions.Add(type, definition);

            return this;
        }

        protected List<DataDefinition> GetDataDefinitions()
        {
            return _dataDefinitions.Values.ToList();
        }
    }
}
