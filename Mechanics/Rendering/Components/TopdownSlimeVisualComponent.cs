namespace ECS_Base.Mechanics.Rendering.Components
{
    /// <summary>
    /// Marks a top-down chaser enemy as using the slime sprite sheet.
    /// </summary>
    public struct TopdownSlimeVisualComponent
    {
        public SlimeFacing Facing;
        public float FrameTimer;
        public bool UseAltFrame;

        public TopdownSlimeVisualComponent(SlimeFacing facing = SlimeFacing.Down)
        {
            Facing = facing;
            FrameTimer = 0f;
            UseAltFrame = false;
        }
    }

    public enum SlimeFacing
    {
        Down,
        Right,
        Left,
        Up
    }
}
