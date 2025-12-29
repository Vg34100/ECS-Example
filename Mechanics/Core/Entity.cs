// Mechanics/Core/Entity.cs
namespace ECS_Base.Mechanics.Core
{
    public class Entity
    {
        public int Id { get; private set; }
        private static int _nextId = 0;

        public Entity()
        {
            Id = _nextId++;
        }
    }
}
