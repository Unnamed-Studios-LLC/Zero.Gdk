using System;
using System.Collections.Generic;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public abstract class ViewQuery
    {
        private bool _started;

        internal void AddEntities(Connection connection)
        {
            AddEntities(connection, connection.View.QueryEntities);
        }

        internal void NewWorld()
        {
            _started = false;
        }

        protected abstract void OnAddEntities(Connection connection, List<uint> entityList);
        protected virtual void OnStartWorld(Connection connection) { }

        private void AddEntities(Connection connection, List<uint> entityList)
        {
            if (!_started)
            {
                _started = true;
                try
                {
                    OnStartWorld(connection);
                }
                catch (Exception e)
                {
                    Debug.LogError(e, "An error occurred during {0}", nameof(OnStartWorld));
                }
            }

            try
            {
                OnAddEntities(connection, entityList);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(OnAddEntities));
            }
        }
    }
}
