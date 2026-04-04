namespace ECS_Base.Mechanics.Gravity.Components
{
    /// <summary>
    /// Component for entities affected by gravity
    /// </summary>
    public struct GravityComponent
    {
        public float GravityScale;

        public GravityComponent(float gravityScale = 1.0f)
        {
            GravityScale = gravityScale;
        }
    }
}
