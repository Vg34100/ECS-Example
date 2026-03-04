namespace ECS_Base.Mechanics.UI.Components
{
    /// <summary>
    /// Binds a text UI element to a player counter (coins, stars, rupees, score).
    /// </summary>
    public struct UICounterComponent
    {
        public int TargetEntityId;
        public CounterType Type;
        public string Prefix;

        public UICounterComponent(int targetEntityId, CounterType type, string prefix)
        {
            TargetEntityId = targetEntityId;
            Type = type;
            Prefix = prefix;
        }
    }

    public enum CounterType
    {
        Coins,
        Stars,
        Rupees,
        Score
    }
}
