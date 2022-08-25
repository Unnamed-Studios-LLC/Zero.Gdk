namespace Zero.Game.Common
{
    public abstract class ViewAction
    {
        private int _aquireCount = 0;

        public abstract ViewActionType ActionType { get; }

        public void Aquire()
        {
            _aquireCount++;
        }

        public void ReturnToCache()
        {
            if (--_aquireCount != 0)
            {
                return;
            }

            ReturnItemsToCache();
            ViewActionCache.Return(this);
        }

        public abstract void Write(ISWriter writer);

        protected abstract void ReturnItemsToCache();

        protected abstract void Read(ISReader reader);
    }
}
