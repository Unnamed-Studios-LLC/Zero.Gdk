namespace Zero.Game.Server
{
    public class ComponentBuilder<T>
        where T : Component
    {
        private readonly ushort _type;
        private int _priority;

        public ComponentBuilder(ushort type)
        {
            _type = type;
        }

        public ComponentBuilder<T> Priority(int priority)
        {
            _priority = priority;
            return this;
        }

        internal ComponentDefinition<T> Build()
        {
            return new ComponentDefinition<T>(_type, _priority);
        }
    }
}
