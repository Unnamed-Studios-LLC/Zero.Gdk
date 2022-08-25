using System;
using System.Collections.Generic;
using System.Linq;
using Zero.Game.Shared;

namespace Zero.Game.Common
{
    public abstract class CommonSchema
    {
        private readonly Dictionary<ushort, DataDefinition> _dataDefinitions;
        private readonly Dictionary<ushort, Func<IData>> _dataFactories;

        public CommonSchema(List<DataDefinition> dataDefinitions)
        {
            _dataDefinitions = dataDefinitions.ToDictionary(x => x.Type);
            _dataFactories = dataDefinitions.ToDictionary(x => x.Type, x => (Func<IData>)x.Create);
        }

        public IData CreateData(ushort type)
        {
            if (!_dataFactories.TryGetValue(type, out var factory))
            {
                return null;
            }
            return factory();
        }

        public DataDefinition GetDataDefinition(ushort type)
        {
            if (!_dataDefinitions.TryGetValue(type, out var definition))
            {
                return null;
            }
            return definition;
        }
    }
}
