namespace ECS_Base.Mechanics.PlayerController.Components
{
    /// <summary>
    /// Component to mark an entity as the player with movement parameters
    /// </summary>
    public struct PlayerComponent
    {
        public float MoveSpeed;
        public float JumpForce;

        public PlayerComponent(float moveSpeed = 150f, float jumpForce = 300f)
        {
            MoveSpeed = moveSpeed;
            JumpForce = jumpForce;
        }
    }
}
