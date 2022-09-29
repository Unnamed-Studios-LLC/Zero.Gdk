using System;
using System.Runtime.CompilerServices;

namespace Zero.Game.Shared
{
    internal abstract class DataHandler
    {
        public abstract bool HandleData(ref BlitReader reader);
        public abstract bool HandleRawData(ref RawBlitReader reader);
        public abstract void SetImplementation(object @object);
    }

    internal unsafe sealed class DataHandler<T> : DataHandler where T : unmanaged
    {
        private IDataHandler<T> _implementation;

        public override bool HandleData(ref BlitReader reader)
        {
            T data = default;
            if (!reader.Read(&data))
            {
                return false;
            }

            try
            {
                _implementation?.HandleData(ref data);
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(HandleData));
            }
            return true;
        }

        public override bool HandleRawData(ref RawBlitReader reader)
        {
            T* data = default;
            if (!reader.Read(&data))
            {
                return false;
            }

            try
            {
                _implementation?.HandleData(ref Unsafe.AsRef<T>(data));
            }
            catch (Exception e)
            {
                Debug.LogError(e, "An error occurred during {0}", nameof(HandleData));
            }
            return true;
        }

        public override void SetImplementation(object @object)
        {
            _implementation = @object as IDataHandler<T>;
        }
    }
}
