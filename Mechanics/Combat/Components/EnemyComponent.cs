namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Marks an entity as an enemy
    /// Used for targeting and AI behavior
    /// </summary>
    public struct EnemyComponent
    {
        public float ChaseSpeed;
        public float ChaseRange; // How far they can see the player
        public float StopDistance; // How close they get before stopping
        public float SeparationRadius; // Distance to keep from other enemies

        // Shooting
        public bool CanShoot;
        public float ShootRange;
        public float ShootCooldown;
        public float TimeSinceLastShot;

        public EnemyComponent(float chaseSpeed = 60f, float chaseRange = 200f, float stopDistance = 40f,
                            float separationRadius = 20f, bool canShoot = false, float shootRange = 150f,
                            float shootCooldown = 1.5f)
        {
            ChaseSpeed = chaseSpeed;
            ChaseRange = chaseRange;
            StopDistance = stopDistance;
            SeparationRadius = separationRadius;
            CanShoot = canShoot;
            ShootRange = shootRange;
            ShootCooldown = shootCooldown;
            TimeSinceLastShot = 0f;
        }
    }
}
