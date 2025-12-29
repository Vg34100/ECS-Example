namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Health component - entities with this can take damage and die
    /// </summary>
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
