namespace Zero.Game.Server
{
    public class ComponentBuilder<T>
        where T : Component
    {
        private int _priority;

        public ComponentBuilder()
        {

        }

        public ComponentBuilder<T> Priority(int priority)
        {
            _priority = priority;
            return this;
        }

        internal ComponentDefinition<T> Build()
        {
            return new ComponentDefinition<T>(_priority);
        }
    }
}
