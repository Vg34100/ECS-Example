using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Marks an entity as a projectile that can damage enemies or players
    /// Projectiles are automatically destroyed on collision
    /// </summary>
    public struct ProjectileComponent
    {
        public enum ProjectileOwner
        {
            Player,
            Enemy
        }

        public int Damage;
        public float Lifetime; // How long before auto-destroy (seconds)
        public float TimeAlive;
        public Vector2 InitialVelocity; // Store initial velocity to detect collisions
        public ProjectileOwner Owner; // Who shot this projectile

        public ProjectileComponent(int damage = 1, float lifetime = 3.0f, Vector2 initialVelocity = default,
                                  ProjectileOwner owner = ProjectileOwner.Player)
        {
            Damage = damage;
            Lifetime = lifetime;
            TimeAlive = 0f;
            InitialVelocity = initialVelocity;
            Owner = owner;
        }
    }
}
