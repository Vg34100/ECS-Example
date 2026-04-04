using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Platformer.Components
{
    /// <summary>
    /// Component for detecting ground beneath entity
    /// </summary>
    public struct GroundSensorComponent
    {
        /// <summary>Offset from entity position to check</summary>
        public Vector2 Offset;

        /// <summary>Width of ground check ray</summary>
        public float Width;

        /// <summary>Distance to check below entity</summary>
        public float CheckDistance;

        /// <summary>Layer mask for what counts as ground</summary>
        public int GroundLayer;

        public GroundSensorComponent(Vector2 offset, float width, float checkDistance)
        {
            Offset = offset;
            Width = width;
            CheckDistance = checkDistance;
            GroundLayer = 0; // Default layer
        }
    }
}
