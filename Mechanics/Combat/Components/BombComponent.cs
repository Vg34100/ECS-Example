namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Bomb that explodes after a timer.
    /// </summary>
    public struct BombComponent
    {
        public float TimeToExplode;
        public float Radius;
        public float Damage;
        public bool Exploded;

        public BombComponent(float timeToExplode, float radius, float damage)
        {
            TimeToExplode = timeToExplode;
            Radius = radius;
            Damage = damage;
            Exploded = false;
        }
    }
}
