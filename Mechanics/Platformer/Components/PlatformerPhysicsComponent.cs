namespace ECS_Base.Mechanics.Platformer.Components
{
    /// <summary>
    /// Component for platformer-specific physics
    /// </summary>
    public struct PlatformerPhysicsComponent
    {
        /// <summary>Jump force applied when jumping</summary>
        public float JumpForce;

        /// <summary>Gravity acceleration</summary>
        public float Gravity;

        /// <summary>Maximum fall speed</summary>
        public float MaxFallSpeed;

        /// <summary>Is entity currently grounded?</summary>
        public bool IsGrounded;

        /// <summary>Was grounded last frame?</summary>
        public bool WasGrounded;

        /// <summary>Time since left ground (for coyote time)</summary>
        public float TimeSinceGrounded;

        /// <summary>Coyote time duration (grace period after leaving ground)</summary>
        public float CoyoteTime;

        /// <summary>Jump buffer time (remember jump input)</summary>
        public float JumpBufferTime;

        /// <summary>Time since jump was pressed</summary>
        public float TimeSinceJumpPressed;

        /// <summary>Is currently jumping?</summary>
        public bool IsJumping;

        /// <summary>Can perform variable height jump?</summary>
        public bool VariableJumpHeight;

        /// <summary>Minimum jump height multiplier</summary>
        public float MinJumpMultiplier;

        /// <summary>Number of additional air jumps available</summary>
        public int MaxAirJumps;

        /// <summary>Current air jumps remaining</summary>
        public int AirJumpsRemaining;

        /// <summary>Can jump right now? (considers coyote time and jump buffer)</summary>
        public readonly bool CanJump =>
            (IsGrounded || TimeSinceGrounded < CoyoteTime || AirJumpsRemaining > 0) &&
            !IsJumping;

        /// <summary>Has buffered jump input?</summary>
        public readonly bool HasJumpBuffer => TimeSinceJumpPressed < JumpBufferTime;

        public PlatformerPhysicsComponent(float jumpForce, float gravity)
        {
            JumpForce = jumpForce;
            Gravity = gravity;
            MaxFallSpeed = 500f;
            IsGrounded = false;
            WasGrounded = false;
            TimeSinceGrounded = 0f;
            CoyoteTime = 0.15f;
            JumpBufferTime = 0.1f;
            TimeSinceJumpPressed = 999f;
            IsJumping = false;
            VariableJumpHeight = true;
            MinJumpMultiplier = 0.5f;
            MaxAirJumps = 0;
            AirJumpsRemaining = 0;
        }
    }
}
