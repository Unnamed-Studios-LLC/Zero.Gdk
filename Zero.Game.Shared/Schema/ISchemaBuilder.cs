using System;

namespace Zero.Game.Shared
{
    public interface ISchemaBuilder
    {
        ISchemaBuilder Data<T>(Action<IDataBuilder<T>> buildAction = null)
            where T : IData, new();
    }
}
