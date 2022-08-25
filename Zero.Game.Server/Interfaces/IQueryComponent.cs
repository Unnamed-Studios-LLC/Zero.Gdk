using System;
using System.Collections.Generic;

namespace Zero.Game.Server
{
    public interface IQueryComponent
    {
        IEnumerable<Entity> GetEntities();

        internal IEnumerable<Entity> GetEntitiesSafe()
        {
            try
            {
                return GetEntities();
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(GetEntities));
                return Array.Empty<Entity>();
            }
        }
    }
}
