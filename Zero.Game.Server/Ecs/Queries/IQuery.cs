namespace Zero.Game.Server
{
    internal unsafe interface IQuery
    {
        void AddRequiredArchetypes(ref EntityArchetype archetype, ref int maxDepth);
        void Func(EntityGroup group, int chunkIndex, int* indices);
        void GetComponentListIndex(EntityGroup group, int* indices);
    }
}
