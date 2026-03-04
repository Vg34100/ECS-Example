namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Allows blocking to reduce or negate damage.
    /// </summary>
    public struct ShieldComponent
    {
        public float DamageMultiplier;

        public ShieldComponent(float damageMultiplier = 0f)
        {
            DamageMultiplier = damageMultiplier;
        }
    }
}
