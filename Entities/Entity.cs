// Entities/Entity.cs
namespace ECS_Example.Entities
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