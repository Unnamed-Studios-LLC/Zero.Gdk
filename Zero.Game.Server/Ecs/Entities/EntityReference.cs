namespace Zero.Game.Server
{
    internal unsafe struct EntityReference
    {
        public EntityReference(EntityGroup group, int chunkIndex, int listIndex)
        {
            Group = group;
            ChunkIndex = chunkIndex;
            ListIndex = listIndex;
        }

        public EntityGroup Group { get; }
        public int ChunkIndex { get; }
        public int ListIndex { get; }

        public ref T GetComponent<T>(int type) where T : unmanaged
        {
            var componentListIndex = -1;
            for (int i = 0; i < Group.NonZeroComponentListCount; i++)
            {
                if (Group.NonZeroComponentTypes[i] == type)
                {
                    componentListIndex = i;
                    break;
                }
            }
            return ref Group.GetComponentRef<T>(ChunkIndex, componentListIndex, ListIndex);
        }
    }
}
