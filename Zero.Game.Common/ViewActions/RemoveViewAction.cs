using System.Collections.Generic;

namespace Zero.Game.Common
{
    public class RemoveViewAction : ViewAction
    {
        public override ViewActionType ActionType => ViewActionType.Remove;

        public uint RemovedEntitiesCount { get; private set; }
        public uint[] RemovedEntities { get; private set; }

        internal RemoveViewAction()
        {

        }

        public override void Write(ISWriter writer)
        {
            writer.WriteArrayLength(RemovedEntitiesCount);
            for (int i = 0; i < RemovedEntitiesCount; i++)
            {
                writer.Write(RemovedEntities[i]);
            }
        }

        protected override void Read(ISReader reader)
        {
            RemovedEntitiesCount = reader.ReadArrayLength();
            RemovedEntities = ListCache.GetUintArray();
            for (int i = 0; i < RemovedEntitiesCount; i++)
            {
                RemovedEntities[i] = reader.ReadUInt32();
            }
        }

        protected override void ReturnItemsToCache()
        {
            ListCache.ReturnUintArray(RemovedEntities);
        }

        public void Assign(uint count, uint[] removedEntities)
        {
            RemovedEntitiesCount = count;
            RemovedEntities = removedEntities;
        }

        internal void Assign(ISReader reader)
        {
            Read(reader);
        }
    }
}
