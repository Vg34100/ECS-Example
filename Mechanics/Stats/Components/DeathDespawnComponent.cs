namespace ECS_Base.Mechanics.Stats.Components
{
    /// <summary>
    /// Delays entity removal so death visuals can be seen briefly.
    /// </summary>
    public struct DeathDespawnComponent
    {
        public float TimeRemaining;

        public DeathDespawnComponent(float timeRemaining)
        {
            TimeRemaining = timeRemaining;
        }
    }
}
