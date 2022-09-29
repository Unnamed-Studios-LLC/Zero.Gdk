using NUnit.Framework;
using System.Linq;
using Zero.Game.Server;

namespace Zero.Game.Tests
{
    public class ViewTests
    {
        [TestCase(
            new uint[] { 1, 2, 4, 6, 7, 9 },
            new uint[] { 1, 4, 5, 8, 9 },
            new uint[] { 2, 6, 7 },
            new uint[] { 5, 8 }
        )]
        [TestCase(
            new uint[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
            new uint[] { 3, 6 },
            new uint[] { 1, 2, 4, 5, 7, 8, 9 },
            new uint[] {  }
        )]
        [TestCase(
            new uint[] { 8, 9 },
            new uint[] { 1, 2, 3 },
            new uint[] { 8, 9 },
            new uint[] { 1, 2, 3 }
        )]
        public void Populate(uint[] lastEntities, uint[] entities, uint[] removedEntities, uint[] newEntities)
        {
            var view = new View();
            for (int i = 0; i < lastEntities.Length; i++)
            {
                view.LastEntities.Add(lastEntities[i]);
            }

            for (int i = 0; i < entities.Length; i++)
            {
                view.QueryEntities.Add(entities[i]);
            }
            view.Populate();

            Assert.IsTrue(view.RemovedEntities.Count == removedEntities.Length);
            for (int i = 0; i < removedEntities.Length; i++)
            {
                Assert.IsTrue(view.RemovedEntities.Contains(removedEntities[i]));
            }

            foreach (var entity in view.UniqueEntities)
            {
                var @new = view.NewEntities.Contains(entity);
                var contains = newEntities.Contains(entity);
                if (@new)
                {
                    Assert.IsTrue(contains);
                }
                else
                {
                    Assert.IsFalse(contains);
                }
            }
        }
    }
}
