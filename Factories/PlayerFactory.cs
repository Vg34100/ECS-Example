// Factories/PlayerFactory.cs
using Microsoft.Xna.Framework;
using ECS_Example.Components;
using ECS_Example.Entities;

namespace ECS_Example.Factories
{
    public static class PlayerFactory
    {
        public static Entity CreatePlayer(World world, Vector2 position)
        {
            var player = world.CreateEntity();

            // Position and visuals
            world.AddComponent(player, new Position(position.X, position.Y));
            world.AddComponent(player, new Shape(
                Shape.ShapeType.Rectangle,
                Color.Green,
                new Vector2(32, 48)
            ));

            // Physics
            world.AddComponent(player, new Velocity(0, 0));
            world.AddComponent(player, new Gravity(980));
            world.AddComponent(player, new Collider(
                new Rectangle(0, 0, 32, 48),
                Collider.ColliderType.Dynamic
            ));

            // Player-specific
            world.AddComponent(player, new PlayerController(200));
            world.AddComponent(player, new Jump(-500));

            // Combat
            world.AddComponent(player, new Health(3));
            world.AddComponent(player, new Damager(1, DamageType.FromAbove));
            world.AddComponent(player, new InvulnerableSettings(1.0f));
            world.AddComponent(player, new StunSettings(0.5f));

            // Attack component
            world.AddComponent(player, new Attack(
                damage: 1,
                hitboxSize: new Vector2(50, 80),  // Square attack hitbox
                hitboxOffsetX: 0,                 // Not needed with our new positioning logic
                cooldown: 0.3f,
                duration: 0.15f                   // Attack persists for 0.15 seconds
            ));

            // Camera target
            world.AddComponent(player, new CameraTarget(true));

            return player;
        }
    }
}