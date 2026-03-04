namespace ECS_Base.Mechanics.Progression.Components
{
    /// <summary>
    /// Marks an entity as a collectible pickup.
    /// </summary>
    public struct CollectibleComponent
    {
        public CollectibleType Type;
        public int Amount;
        public int ScoreValue;

        public CollectibleComponent(CollectibleType type, int amount = 1, int scoreValue = 0)
        {
            Type = type;
            Amount = amount;
            ScoreValue = scoreValue;
        }
    }

    public enum CollectibleType
    {
        Coin,
        Star,
        Rupee,
        Arrow,
        Bomb
    }
}
