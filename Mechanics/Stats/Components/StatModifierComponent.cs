namespace ECS_Base.Mechanics.Stats.Components
{
    /// <summary>
    /// Temporary stat modifier (buff/debuff)
    /// </summary>
    public struct StatModifierComponent
    {
        /// <summary>Type of stat to modify</summary>
        public StatType StatType;

        /// <summary>Modifier value (additive)</summary>
        public float Value;

        /// <summary>Modifier multiplier (multiplicative, 1.0 = no change)</summary>
        public float Multiplier;

        /// <summary>Duration in seconds (0 = permanent)</summary>
        public float Duration;

        /// <summary>Time remaining</summary>
        public float TimeRemaining;

        /// <summary>Unique identifier for this modifier</summary>
        public string Id;

        /// <summary>Is this modifier expired?</summary>
        public readonly bool IsExpired => Duration > 0 && TimeRemaining <= 0;

        public StatModifierComponent(StatType statType, float value = 0f, float multiplier = 1f, float duration = 0f, string id = "")
        {
            StatType = statType;
            Value = value;
            Multiplier = multiplier;
            Duration = duration;
            TimeRemaining = duration;
            Id = id;
        }
    }

    public enum StatType
    {
        Health,
        MaxHealth,
        Attack,
        Defense,
        Speed
    }
}
