using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Stats.Components
{
    /// <summary>
    /// Adds a damage flash when taking damage.
    /// </summary>
    public struct DamageFlashOnHitComponent
    {
        public Color FlashColor;
        public float Duration;

        public DamageFlashOnHitComponent(Color flashColor, float duration)
        {
            FlashColor = flashColor;
            Duration = duration;
        }
    }
}
