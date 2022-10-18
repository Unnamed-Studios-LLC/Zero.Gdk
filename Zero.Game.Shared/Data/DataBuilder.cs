using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Zero.Game.Shared
{
    public class DataBuilder
    {
        private const int Capacity = byte.MaxValue + 1;

        private readonly List<DataDefinition> _definitions = new List<DataDefinition>();
        private readonly HashSet<Type> _types = new HashSet<Type>();

        public DataBuilder Define<T>() where T : unmanaged
        {
            var type = typeof(T);
            if (!_types.Add(type))
            {
                throw new Exception($"Data {type.FullName} has already been defined");
            }

            if (_definitions.Count >= Capacity)
            {
                throw new Exception($"Max data types reached. Only {Capacity} data types may be defined");
            }

            if (type.StructLayoutAttribute == null)
            {
                throw new Exception($"Data {type.FullName} missing StructLayout attribute");
            }

            if (type.StructLayoutAttribute.Value != LayoutKind.Explicit)
            {
                throw new Exception($"Data {type.FullName} LayoutKind of StructLayout attribute must be set to Explicit");
            }

            Data<T>.Generate();
            _definitions.Add(new DataDefinition<T>());
            return this;
        }

        internal DataDefinition[] Build()
        {
            return _definitions.ToArray();
        }
    }
}