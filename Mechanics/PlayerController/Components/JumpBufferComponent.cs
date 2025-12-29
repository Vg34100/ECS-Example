namespace ECS_Base.Mechanics.PlayerController.Components
{
    /// <summary>
    /// Jump buffering - remembers jump input for a short time before landing
    /// If you press jump slightly before hitting the ground, it will still jump when you land
    /// </summary>
    public struct JumpBufferComponent
    {
        public float BufferWindow; // How long to remember a jump press (in seconds)
        public float TimeSinceJumpPressed; // Time since jump was pressed
        public bool JumpBuffered; // Is there a buffered jump waiting?

        public JumpBufferComponent(float bufferWindow = 0.1f)
        {
            BufferWindow = bufferWindow;
            TimeSinceJumpPressed = 999f; // Start with a large value
            JumpBuffered = false;
        }
    }
}
