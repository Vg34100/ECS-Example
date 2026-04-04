namespace ECS_Base.Mechanics.Stats.Components
{
    /// <summary>
    /// Core stats component for entities (health, damage, defense, etc.)
    /// </summary>
    public struct StatsComponent
    {
        /// <summary>Current health</summary>
        public float Health;

        /// <summary>Maximum health</summary>
        public float MaxHealth;

        /// <summary>Base attack damage</summary>
        public float Attack;

        /// <summary>Defense/armor value (reduces incoming damage)</summary>
        public float Defense;

        /// <summary>Movement speed multiplier</summary>
        public float Speed;

        /// <summary>Is this entity dead?</summary>
        public bool IsDead;

        /// <summary>Is this entity invulnerable? (takes no damage)</summary>
        public bool IsInvulnerable;

        /// <summary>Health percentage (0.0 to 1.0)</summary>
        public readonly float HealthPercentage => MaxHealth > 0 ? Health / MaxHealth : 0f;

        /// <summary>Is health at maximum?</summary>
        public readonly bool IsFullHealth => Health >= MaxHealth;

        /// <summary>Is health critically low? (below 25%)</summary>
        public readonly bool IsCriticalHealth => HealthPercentage < 0.25f;

        public StatsComponent(float maxHealth, float attack = 10f, float defense = 0f, float speed = 1f)
        {
            MaxHealth = maxHealth;
            Health = maxHealth;
            Attack = attack;
            Defense = defense;
            Speed = speed;
            IsDead = false;
            IsInvulnerable = false;
        }

        /// <summary>
        /// Take damage and return actual damage dealt (after defense)
        /// </summary>
        public float TakeDamage(float damage)
        {
            if (IsInvulnerable || IsDead)
                return 0f;

            // Calculate damage after defense (defense reduces damage by percentage)
            float damageMultiplier = 100f / (100f + Defense);
            float actualDamage = damage * damageMultiplier;

            Health -= actualDamage;

            if (Health <= 0)
            {
                Health = 0;
                IsDead = true;
            }

            return actualDamage;
        }

        /// <summary>
        /// Heal and return actual amount healed
        /// </summary>
        public float Heal(float amount)
        {
            if (IsDead)
                return 0f;

            float oldHealth = Health;
            Health = System.Math.Min(Health + amount, MaxHealth);
            return Health - oldHealth;
        }
    }
}
