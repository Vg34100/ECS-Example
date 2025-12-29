namespace ECS_Base.Mechanics.Collision.Components
{
    /// <summary>
    /// Component to track if an entity is currently grounded (touching ground)
    /// </summary>
    public struct GroundedComponent
    {
        public bool IsGrounded;
        public bool TouchingLeft;
        public bool TouchingRight;
        public bool TouchingTop;

        public GroundedComponent(bool isGrounded = false)
        {
            IsGrounded = isGrounded;
            TouchingLeft = false;
            TouchingRight = false;
            TouchingTop = false;
        }
    }
}
