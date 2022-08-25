using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Zero.Game.Common
{
    public static class ViewActionBatchCache
    {
        private readonly static ConcurrentQueue<ViewActionBatch> _batches = new ConcurrentQueue<ViewActionBatch>();

        public static ViewActionBatch Get(uint batchId, uint time, List<ViewAction> actions)
        {
            var batch = Get();
            batch.Assign(batchId, time, actions);
            return batch;
        }

        public static ViewActionBatch Get(ISReader reader)
        {
            var batch = Get();
            batch.Assign(reader);
            return batch;
        }

        public static void Return(ViewActionBatch batch)
        {
            batch.ReturnItemsToCache();
            _batches.Enqueue(batch);
        }

        internal static ViewActionBatch Get()
        {
            if (_batches.TryDequeue(out var batch))
            {
                return batch;
            }

            return new ViewActionBatch();
        }
    }
}
