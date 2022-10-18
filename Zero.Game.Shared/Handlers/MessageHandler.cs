using System;

namespace Zero.Game.Shared
{
    internal sealed class MessageHandler
    {
        private readonly DataHandler[] _handlers = new DataHandler[SharedDomain.DataDefinitions.Length];

        public MessageHandler()
        {
            for (int i = 0; i < SharedDomain.DataDefinitions.Length; i++)
            {
                _handlers[i] = SharedDomain.DataDefinitions[i].GetHandler();
            }
        }

        public IMessageHandler Implementation { get; private set; }

        public bool HandleData(byte type, ref BlitReader reader)
        {
            if (type > _handlers.Length)
            {
                return false;
            }

            var handler = _handlers[type];
            return handler.HandleData(ref reader);
        }

        public bool HandleRawData(byte type, ref RawBlitReader reader)
        {
            if (type > _handlers.Length)
            {
                return false;
            }

            var handler = _handlers[type];
            return handler.HandleRawData(ref reader);
        }

        public void HandleEntity(uint entityId)
        {
            try
            {
                Implementation?.HandleEntity(entityId);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(HandleEntity));
            }
        }

        public void HandleRemove(uint entityId)
        {
            try
            {
                Implementation?.HandleRemove(entityId);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(HandleRemove));
            }
        }

        public void HandleWorld(uint worldId)
        {
            try
            {
                Implementation?.HandleWorld(worldId);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(HandleWorld));
            }
        }

        public void PreHandle(uint time)
        {
            try
            {
                Implementation?.PreHandle(time);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(PreHandle));
            }
        }

        public void PostHandle()
        {
            try
            {
                Implementation?.PostHandle();
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(PostHandle));
            }
        }

        public void SetImplementation(IMessageHandler implementation)
        {
            Implementation = implementation;
            for (int i = 0; i < _handlers.Length; i++)
            {
                _handlers[i].SetImplementation(implementation);
            }
        }
    }
}
