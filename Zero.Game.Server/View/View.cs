using System.Collections.Generic;

namespace Zero.Game.Server
{
    internal class View
    {
        public List<uint> QueryEntities { get; private set; } = new(100);
        public HashSet<uint> UniqueEntities { get; private set; } = new(100);
        public HashSet<uint> ProcessedEntities { get; private set; } = new(100);
        public HashSet<uint> LastEntities { get; private set; } = new(100);
        public HashSet<uint> NewEntities { get; } = new(100);
        public List<uint> RemovedEntities { get; } = new(100);

        public void StageEntities()
        {
            (UniqueEntities, LastEntities) = (LastEntities, UniqueEntities);
            UniqueEntities.Clear();
            QueryEntities.Clear();
        }

        public void Populate()
        {
            foreach (var entityId in QueryEntities)
            {
                UniqueEntities.Add(entityId);
                if (!LastEntities.Contains(entityId))
                {
                    NewEntities.Add(entityId);
                }
            }

            foreach (var entityId in LastEntities)
            {
                if (!UniqueEntities.Contains(entityId))
                {
                    RemovedEntities.Add(entityId);
                }
            }
        }

        public void PostSend()
        {
            RemovedEntities.Clear();
            NewEntities.Clear();
        }
    }
}
