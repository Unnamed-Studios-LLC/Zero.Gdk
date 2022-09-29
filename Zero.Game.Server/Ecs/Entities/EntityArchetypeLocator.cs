using System.Collections.Generic;

namespace Zero.Game.Server
{
    internal sealed unsafe class EntityArchetypeLocator
    {
        private readonly Dictionary<ulong, EntityArchetypeLocator> _locators = new();

        private int GroupIndex { get; set; } = -1;

        public void Insert(ulong* archetypes, int depth, int groupIndex)
        {
            int i = 0;
            EntityArchetypeLocator locator = this;
            while (i < depth)
            {
                if (!locator._locators.TryGetValue(archetypes[i++], out locator))
                {
                    locator = new EntityArchetypeLocator();
                }
            }
            locator.GroupIndex = groupIndex;
        }

        public bool TryResolve(ulong* archetypes, int depth, out int groupIndex)
        {
            int i = 0;
            EntityArchetypeLocator locator = this;
            while (locator._locators.TryGetValue(archetypes[i++], out locator))
            {
                if (i == depth)
                {
                    groupIndex = locator.GroupIndex;
                    return groupIndex != -1;
                }
            }

            groupIndex = -1;
            return false;
        }
    }
}
