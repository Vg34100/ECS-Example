namespace ECS_Base.Mechanics.Movement.Components
{
    /// <summary>
    /// Component for top-down 8-directional movement (no gravity)
    /// Use this instead of platformer physics for games like Isaac, Zelda, etc.
    /// </summary>
    public struct TopDownMovementComponent
    {
        public float MoveSpeed;

        public TopDownMovementComponent(float moveSpeed = 100f)
        {
            MoveSpeed = moveSpeed;
        }
    }
}
