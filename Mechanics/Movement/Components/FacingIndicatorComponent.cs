using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Movement.Components
{
    /// <summary>
    /// Places a small indicator in front of a target entity.
    /// </summary>
    public struct FacingIndicatorComponent
    {
        public int TargetEntityId;
        public float Distance;
        public Vector2 Offset;

        public FacingIndicatorComponent(int targetEntityId, float distance, Vector2 offset)
        {
            TargetEntityId = targetEntityId;
            Distance = distance;
            Offset = offset;
        }
    }
}
