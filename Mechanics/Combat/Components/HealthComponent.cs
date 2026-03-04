namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Health component - deprecated. Use StatsComponent instead.
    /// </summary>
    [System.Obsolete("Use StatsComponent for health and combat.")]
    public struct HealthComponent
    {
        public int CurrentHealth;
        public int MaxHealth;

        public HealthComponent(int maxHealth = 3)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }
    }
}
