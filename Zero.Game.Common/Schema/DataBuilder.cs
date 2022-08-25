using System;
using Zero.Game.Shared;

namespace Zero.Game.Common
{
    public class DataBuilder<T> : IDataBuilder<T>
        where T : IData, new()
    {
        private readonly ushort _type;
        private bool _persist;
        private Func<T, bool> _defaultCompare;

        public DataBuilder(ushort type)
        {
            _type = type;
        }

        public IDataBuilder<T> IsDefault(Func<T, bool> defaultCompare)
        {
            _defaultCompare = defaultCompare;
            return this;
        }

        public IDataBuilder<T> Persist()
        {
            _persist = true;
            return this;
        }

        public IDataBuilder<T> Persist(bool value)
        {
            _persist = value;
            return this;
        }

        internal DataDefinition<T> Build()
        {
            return new DataDefinition<T>(_type, _persist, _defaultCompare);
        }
    }
}
