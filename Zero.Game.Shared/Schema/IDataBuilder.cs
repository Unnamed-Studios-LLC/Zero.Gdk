using System;

namespace Zero.Game.Shared
{
    public interface IDataBuilder<T> where T : IData, new()
    {
        IDataBuilder<T> IsDefault(Func<T, bool> defaultCompare);
        IDataBuilder<T> Persist();
        IDataBuilder<T> Persist(bool value);
    }
}
