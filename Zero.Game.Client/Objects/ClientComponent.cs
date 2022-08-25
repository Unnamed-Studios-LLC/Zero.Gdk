using System;
using System.Collections.Generic;
using Zero.Game.Shared;

namespace Zero.Game.Client
{
    public abstract class ClientComponent
    {
        public ZeroClientInstance Client { get; private set; }
        public uint Time { get; internal set; }

        protected virtual void OnConnect()
        {

        }

        protected virtual void OnDisconnect()
        {

        }

        protected virtual void OnPostReceive()
        {

        }

        protected virtual void OnPreReceive()
        {

        }

        protected virtual void OnReceivedEntityData(uint id, List<IData> data)
        {

        }

        protected virtual void OnReceivedEntityRemoved(uint id)
        {

        }

        protected virtual void OnReceviedWorldData(uint id, List<IData> data)
        {

        }

        protected virtual void OnSentEntityData(uint id, List<IData> data)
        {

        }

        protected virtual void OnUpdate()
        {

        }

        internal void Connect()
        {
            try
            {
                OnConnect();
            }
            catch (Exception e)
            {
                ClientDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnConnect));
            }
        }

        internal void Disconnect()
        {
            try
            {
                OnDisconnect();
            }
            catch (Exception e)
            {
                ClientDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnDisconnect));
            }
        }

        internal void PostReceive()
        {
            try
            {
                OnPostReceive();
            }
            catch (Exception e)
            {
                ClientDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnPostReceive));
            }
        }

        internal void PreReceive()
        {
            try
            {
                OnPreReceive();
            }
            catch (Exception e)
            {
                ClientDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnPreReceive));
            }
        }

        internal void ReceivedEntityData(uint id, List<IData> data)
        {
            try
            {
                OnReceivedEntityData(id, data);
            }
            catch (Exception e)
            {
                ClientDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnReceivedEntityData));
            }
        }

        internal void ReceivedEntityRemoved(uint id)
        {
            try
            {
                OnReceivedEntityRemoved(id);
            }
            catch (Exception e)
            {
                ClientDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnReceivedEntityRemoved));
            }
        }

        internal void ReceivedWorldData(uint id, List<IData> data)
        {
            try
            {
                OnReceviedWorldData(id, data);
            }
            catch (Exception e)
            {
                ClientDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnReceviedWorldData));
            }
        }

        internal void SentEntityData(uint id, List<IData> data)
        {
            try
            {
                OnSentEntityData(id, data);
            }
            catch (Exception e)
            {
                ClientDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnSentEntityData));
            }
        }

        internal void Update()
        {
            try
            {
                OnUpdate();
            }
            catch (Exception e)
            {
                ClientDomain.InternalLog(LogLevel.Error, e, "An error occurred during {0}", nameof(OnUpdate));
            }
        }
    }
}
