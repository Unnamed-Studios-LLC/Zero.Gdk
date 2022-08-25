using System;
using Zero.Game.Shared;

namespace Zero.Game.Common
{
    public abstract class DataDefinition
    {
        protected DataDefinition(ushort type, bool persist)
        {
            Type = type;
            Persist = persist;
        }

        public ushort Type { get; }
        public bool Persist { get; }
        public abstract Type ClassType { get; }

        public abstract IData Create();
        public abstract bool IsDefault(IData data);
    }

    public class DataDefinition<T> : DataDefinition
        where T : IData, new()
    {
        private readonly Func<T, bool> _defaultCompare;

        public DataDefinition(ushort type, bool persist, Func<T, bool> defaultCompare) : base(type, persist)
        {
            _defaultCompare = defaultCompare;
        }

        public override Type ClassType => typeof(T);

        public override IData Create()
        {
            return new T();
        }

        public override bool IsDefault(IData data)
        {
            return _defaultCompare?.Invoke((T)data) ?? false;
        }
    }
}
