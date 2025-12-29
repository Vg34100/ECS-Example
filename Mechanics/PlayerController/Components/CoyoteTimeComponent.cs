namespace ECS_Base.Mechanics.PlayerController.Components
{
    /// <summary>
    /// Coyote time - allows jumping for a short period after leaving a platform
    /// This makes platforming feel more forgiving and responsive
    /// </summary>
    public struct CoyoteTimeComponent
    {
        public float TimeWindow; // How long after leaving ground can you still jump (in seconds)
        public float TimeLeftGround; // Time since we left the ground
        public bool WasGroundedLastFrame;

        public CoyoteTimeComponent(float timeWindow = 0.1f)
        {
            TimeWindow = timeWindow;
            TimeLeftGround = 0f;
            WasGroundedLastFrame = false;
        }
    }
}
