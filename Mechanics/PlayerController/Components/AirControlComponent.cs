namespace ECS_Base.Mechanics.PlayerController.Components
{
    /// <summary>
    /// Air control - allows different movement behavior in the air vs on ground
    /// Can make air movement feel more floaty or give less control while airborne
    /// </summary>
    public struct AirControlComponent
    {
        public float AirControlFactor; // Multiplier for horizontal movement in air (1.0 = same as ground, 0.5 = half control)
        public float AirAcceleration; // How quickly you can change direction in air (0-1, higher = more responsive)

        public AirControlComponent(float airControlFactor = 0.8f, float airAcceleration = 0.5f)
        {
            AirControlFactor = airControlFactor;
            AirAcceleration = airAcceleration;
        }
    }
}
