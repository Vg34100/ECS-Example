namespace ECS_Base.Mechanics.Progression.Components
{
    /// <summary>
    /// Temporary extra health that is removed on hit.
    /// </summary>
    public struct ArmorComponent
    {
        public float ExtraHealth;

        public ArmorComponent(float extraHealth)
        {
            ExtraHealth = extraHealth;
        }
    }
}
