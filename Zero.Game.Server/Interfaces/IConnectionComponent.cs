using System;
using System.Collections.Generic;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    public interface IConnectionComponent
    {
        void OnClientReceivedEntityData(uint time, uint entityId, List<IData> data);

        void OnClientReceivedEntityRemoved(uint time, uint entityId);

        void OnClientReceivedWorldData(uint time, uint worldId, List<IData> data);

        void OnPostReceive(uint time);

        void OnPreReceive(uint time);

        void OnReceivedEntityData(uint time, uint entityId, List<IData> data);

        void OnSentEntityData(uint entityId, List<IData> data);

        void OnSentEntityRemoved(uint entityId);

        void OnSentWorldData(uint worldId, List<IData> data);

        internal void ClientReceivedEntityData(uint time, uint entityId, List<IData> data)
        {
            try
            {
                OnClientReceivedEntityData(time, entityId, data);
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnClientReceivedEntityData));
            }
        }

        internal void ClientReceivedEntityRemoved(uint time, uint entityId)
        {
            try
            {
                OnClientReceivedEntityRemoved(time, entityId);
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnClientReceivedEntityRemoved));
            }
        }

        internal void ClientReceivedWorldData(uint time, uint worldId, List<IData> data)
        {
            try
            {
                OnClientReceivedWorldData(time, worldId, data);
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnClientReceivedWorldData));
            }
        }

        internal void PostReceive(uint time)
        {
            try
            {
                OnPostReceive(time);
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnPostReceive));
            }
        }

        internal void PreReceive(uint time)
        {
            try
            {
                OnPreReceive(time);
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnPreReceive));
            }
        }

        internal void ReceivedData(uint time, uint entityId, List<IData> data)
        {
            try
            {
                OnReceivedEntityData(time, entityId, data);
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnReceivedEntityData));
            }
        }

        internal void SentEntityData(uint entityId, List<IData> data)
        {
            try
            {
                OnSentEntityData(entityId, data);
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnSentEntityData));
            }
        }

        internal void SentEntityRemoved(uint entityId)
        {
            try
            {
                OnSentEntityRemoved(entityId);
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnSentEntityRemoved));
            }
        }

        internal void SentWorldData(uint worldId, List<IData> data)
        {
            try
            {
                OnSentWorldData(worldId, data);
            }
            catch (Exception e)
            {
                ServerDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnSentWorldData));
            }
        }
    }
}
