namespace ECS_Base.Mechanics.Progression.Components
{
    /// <summary>
    /// Powerup pickup that modifies player stats.
    /// </summary>
    public struct PowerupComponent
    {
        public PowerupType Type;
        public float Amount;

        public PowerupComponent(PowerupType type, float amount = 1f)
        {
            Type = type;
            Amount = amount;
        }
    }

    public enum PowerupType
    {
        MaxHealthUp,
        Heal,
        SpeedBoost
    }
}
