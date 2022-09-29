namespace Zero.Game.Server.Ecs.Queries
{
    public struct QueryBuilder
    {
        private readonly Entities _entities;

        internal QueryBuilder(Entities entities)
        {
            _entities = entities;
        }
    }
}
