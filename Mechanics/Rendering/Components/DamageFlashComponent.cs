using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Rendering.Components
{
    /// <summary>
    /// Flashes an entity's ShapeComponent color for damage feedback.
    /// </summary>
    public struct DamageFlashComponent
    {
        public Color OriginalColor;
        public Color FlashColor;
        public float TimeRemaining;
        public float FlashInterval;
        public float TimeSinceToggle;
        public bool FlashOn;

        public DamageFlashComponent(Color originalColor, Color flashColor, float duration, float flashInterval = 0.08f)
        {
            OriginalColor = originalColor;
            FlashColor = flashColor;
            TimeRemaining = duration;
            FlashInterval = flashInterval;
            TimeSinceToggle = 0f;
            FlashOn = true;
        }
    }
}
