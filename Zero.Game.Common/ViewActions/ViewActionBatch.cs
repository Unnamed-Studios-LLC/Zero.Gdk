using System.Collections.Generic;

namespace Zero.Game.Common
{
    public class ViewActionBatch
    {
        public uint BatchId { get; private set; }
        public uint Time { get; private set; }
        public List<ViewAction> Actions { get; private set; }

        internal ViewActionBatch()
        {

        }

        public void Write(ISWriter writer)
        {
            writer.Write(BatchId);
            writer.Write(Time);
            writer.WriteArrayLength((uint)Actions.Count);
            for (int i = 0; i < Actions.Count; i++)
            {
                var action = Actions[i];
                writer.Write((uint)action.ActionType, 2);
                Actions[i].Write(writer);
            }
        }

        private void Read(ISReader reader)
        {
            BatchId = reader.ReadUInt32();
            Time = reader.ReadUInt32();

            var actionsLength = reader.ReadArrayLength();
            Actions = ListCache.GetViewActionList();
            for (int i = 0; i < actionsLength; i++)
            {
                var type = (ViewActionType)reader.Read(2);
                var action = ViewActionCache.GetAction(type, reader);
                action.Aquire();
                Actions.Add(action);
            }
        }

        internal void Assign(uint batchId, uint time, List<ViewAction> actions)
        {
            BatchId = batchId;
            Time = time;
            Actions = actions;
        }

        internal void Assign(ISReader reader)
        {
            Read(reader);
        }

        internal void ReturnItemsToCache()
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                Actions[i].ReturnToCache();
            }
            ListCache.ReturnViewActionList(Actions);
            Actions = null;
        }
    }
}
