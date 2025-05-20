// Components/Health.cs
namespace ECS_Example.Components
{
    public struct Health
    {
        public int CurrentHealth;
        public int MaxHealth;
        public bool IsDead => CurrentHealth <= 0;

        public Health(int maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth < 0)
                CurrentHealth = 0;
        }
    }
}