// Factories/EnemyFactory.cs
using ECS_Example.Components;
using ECS_Example.Entities;
using ECS_Example;
using Microsoft.Xna.Framework;

public static class EnemyFactory
{
    public static Entity CreatePatrolEnemy(World world, Vector2 position, float speed, bool avoidFalling, Color color, int health = 1)
    {
        var enemy = world.CreateEntity();

        world.AddComponent(enemy, new Position(position.X, position.Y));
        world.AddComponent(enemy, new Shape(
            Shape.ShapeType.Rectangle,
            color,
            new Vector2(10, 10)
        ));
        world.AddComponent(enemy, new Velocity(0, 0));
        world.AddComponent(enemy, new Patrol(speed, avoidFalling));
        world.AddComponent(enemy, new Gravity(980));
        world.AddComponent(enemy, new Collider(
            new Rectangle(0, 0, 10, 10),
            Collider.ColliderType.Dynamic
        ));
        world.AddComponent(enemy, new Health(health));
        world.AddComponent(enemy, new Damager(1, DamageType.Contact));
        world.AddComponent(enemy, new Bounceable(-300));

        return enemy;
    }
}