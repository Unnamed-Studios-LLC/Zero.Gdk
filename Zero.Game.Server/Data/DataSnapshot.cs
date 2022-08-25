using System.Collections.Generic;
using Zero.Game.Common;
using Zero.Game.Shared;

namespace Zero.Game.Server
{
    internal class DataSnapshot
    {
        private UpdateViewAction _private;
        private UpdateViewAction _privateUpdated;
        private UpdateViewAction _public;
        private UpdateViewAction _publicUpdated;

        public void Assign(ObjectType objectType, uint id, List<IData> @private, List<IData> privateUpdated, List<IData> @public, List<IData> publicUpdated)
        {
            if (@private != null)
            {
                _private = ViewActionCache.GetUpdate(objectType, id, @private);
                _private.Aquire();
            }

            if (privateUpdated != null)
            {
                _privateUpdated = ViewActionCache.GetUpdate(objectType, id, privateUpdated);
                _privateUpdated.Aquire();
            }

            _public = ViewActionCache.GetUpdate(objectType, id, @public);
            _publicUpdated = ViewActionCache.GetUpdate(objectType, id, publicUpdated);

            _public.Aquire();
            _publicUpdated.Aquire();
        }

        public UpdateViewAction GetUpdate(bool hasAuthority, bool newEntity)
        {
            return newEntity ?
                (!hasAuthority ? _public : (_private ?? _public)) :
                (!hasAuthority ? _publicUpdated : (_privateUpdated ?? _publicUpdated));
        }

        public void ReturnItemsToCache()
        {
            _private?.ReturnToCache();
            _privateUpdated?.ReturnToCache();
            _public.ReturnToCache();
            _publicUpdated.ReturnToCache();
        }
    }
}
