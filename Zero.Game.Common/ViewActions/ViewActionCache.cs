using System.Collections.Concurrent;
using System.Collections.Generic;
using Zero.Game.Shared;

namespace Zero.Game.Common
{
    public static class ViewActionCache
    {
        private static readonly ConcurrentQueue<RemoveViewAction> _removeActions = new ConcurrentQueue<RemoveViewAction>();
        private static readonly ConcurrentQueue<TransferViewAction> _transferActions = new ConcurrentQueue<TransferViewAction>();
        private static readonly ConcurrentQueue<UpdateViewAction> _updateActions = new ConcurrentQueue<UpdateViewAction>();

        public static ViewAction GetAction(ViewActionType type, ISReader reader)
        {
            switch (type)
            {
                case ViewActionType.Remove:
                    return GetRemove(reader);
                case ViewActionType.Transfer:
                    return GetTransfer(reader);
                default:
                    return GetUpdate(reader);
            }
        }

        public static RemoveViewAction GetRemove(uint count, uint[] removedEntities)
        {
            var remove = GetRemove();
            remove.Assign(count, removedEntities);
            return remove;
        }

        public static RemoveViewAction GetRemove(ISReader reader)
        {
            var remove = GetRemove();
            remove.Assign(reader);
            return remove;
        }

        public static TransferViewAction GetTransfer(string ip, ushort port, string key)
        {
            var transfer = GetTransfer();
            transfer.Assign(ip, port, key);
            return transfer;
        }

        public static TransferViewAction GetTransfer(ISReader reader)
        {
            var transfer = GetTransfer();
            transfer.Assign(reader);
            return transfer;
        }

        public static UpdateViewAction GetUpdate(ObjectType objectType, uint id, List<IData> data)
        {
            var update = GetUpdate();
            update.Assign(objectType, id, data);
            return update;
        }

        public static UpdateViewAction GetUpdate(ISReader reader)
        {
            var update = GetUpdate();
            update.Assign(reader);
            return update;
        }

        internal static RemoveViewAction GetRemove()
        {
            if (!_removeActions.TryDequeue(out var remove))
            {
                remove = new RemoveViewAction();
            }
            return remove;
        }

        internal static TransferViewAction GetTransfer()
        {
            if (!_transferActions.TryDequeue(out var transfer))
            {
                transfer = new TransferViewAction();
            }
            return transfer;
        }

        internal static UpdateViewAction GetUpdate()
        {
            if (!_updateActions.TryDequeue(out var update))
            {
                update = new UpdateViewAction();
            }
            return update;
        }

        internal static void Return(ViewAction action)
        {
            switch (action.ActionType)
            {
                case ViewActionType.Remove:
                    _removeActions.Enqueue(action as RemoveViewAction);
                    break;
                case ViewActionType.Transfer:
                    _transferActions.Enqueue(action as TransferViewAction);
                    break;
                case ViewActionType.Update:
                    _updateActions.Enqueue(action as UpdateViewAction);
                    break;
            }
        }
    }
}
