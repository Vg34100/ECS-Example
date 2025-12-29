namespace ECS_Base.Mechanics.PlayerController.Components
{
    /// <summary>
    /// Variable jump height - holding jump longer makes you jump higher
    /// Releasing jump early cuts the jump short for more precise control
    /// </summary>
    public struct VariableJumpComponent
    {
        public float MinJumpTime; // Minimum time jump must be held for minimum jump
        public float MaxJumpTime; // Maximum time jump can be held for max jump
        public float JumpCutMultiplier; // How much to reduce upward velocity when jump is released (0.5 = half speed)
        public float CurrentJumpTime; // How long current jump has been held
        public bool IsJumping; // Are we currently in a jump?

        public VariableJumpComponent(float minJumpTime = 0.1f, float maxJumpTime = 0.4f, float jumpCutMultiplier = 0.5f)
        {
            MinJumpTime = minJumpTime;
            MaxJumpTime = maxJumpTime;
            JumpCutMultiplier = jumpCutMultiplier;
            CurrentJumpTime = 0f;
            IsJumping = false;
        }
    }
}
