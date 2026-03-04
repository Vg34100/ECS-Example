using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Separate damage zone bounds (independent from collision bounds).
    /// </summary>
    public struct DamageZoneComponent
    {
        public Rectangle LocalBounds;

        public DamageZoneComponent(Rectangle localBounds)
        {
            LocalBounds = localBounds;
        }
    }
}
