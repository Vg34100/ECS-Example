using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Platformer.Components
{
    /// <summary>
    /// Types of platform movement patterns
    /// </summary>
    public enum PlatformMovementType
    {
        /// <summary>Linear back-and-forth between two points</summary>
        Linear,

        /// <summary>Circular motion around a center point</summary>
        Circular,

        /// <summary>Follow a series of waypoints</summary>
        Waypoint
    }

    /// <summary>
    /// Component for platforms that move
    /// </summary>
    public struct MovingPlatformComponent
    {
        /// <summary>Type of movement pattern</summary>
        public PlatformMovementType MovementType;

        /// <summary>Speed of movement</summary>
        public float Speed;

        /// <summary>Start position (for Linear)</summary>
        public Vector2 StartPosition;

        /// <summary>End position (for Linear)</summary>
        public Vector2 EndPosition;

        /// <summary>Center point (for Circular)</summary>
        public Vector2 CenterPoint;

        /// <summary>Radius (for Circular)</summary>
        public float Radius;

        /// <summary>Current angle (for Circular)</summary>
        public float Angle;

        /// <summary>Waypoints (for Waypoint movement)</summary>
        public List<Vector2> Waypoints;

        /// <summary>Current waypoint index</summary>
        public int CurrentWaypointIndex;

        /// <summary>Wait time at waypoints</summary>
        public float WaypointWaitTime;

        /// <summary>Current wait timer</summary>
        public float CurrentWaitTime;

        /// <summary>Direction (-1 or 1 for reversing)</summary>
        public int Direction;

        /// <summary>Current velocity of platform</summary>
        public Vector2 Velocity;

        /// <summary>Previous position (for calculating velocity)</summary>
        public Vector2 PreviousPosition;

        /// <summary>Create linear moving platform</summary>
        public static MovingPlatformComponent CreateLinear(Vector2 start, Vector2 end, float speed)
        {
            return new MovingPlatformComponent
            {
                MovementType = PlatformMovementType.Linear,
                StartPosition = start,
                EndPosition = end,
                Speed = speed,
                Direction = 1,
                Velocity = Vector2.Zero,
                PreviousPosition = start,
                WaypointWaitTime = 0f,
                CurrentWaitTime = 0f,
                Waypoints = new List<Vector2>()
            };
        }

        /// <summary>Create circular moving platform</summary>
        public static MovingPlatformComponent CreateCircular(Vector2 center, float radius, float speed)
        {
            return new MovingPlatformComponent
            {
                MovementType = PlatformMovementType.Circular,
                CenterPoint = center,
                Radius = radius,
                Speed = speed,
                Angle = 0f,
                Direction = 1,
                Velocity = Vector2.Zero,
                PreviousPosition = center + new Vector2(radius, 0),
                WaypointWaitTime = 0f,
                CurrentWaitTime = 0f,
                Waypoints = new List<Vector2>()
            };
        }

        /// <summary>Create waypoint moving platform</summary>
        public static MovingPlatformComponent CreateWaypoint(List<Vector2> waypoints, float speed, float waitTime = 0f)
        {
            var firstPoint = waypoints.Count > 0 ? waypoints[0] : Vector2.Zero;
            return new MovingPlatformComponent
            {
                MovementType = PlatformMovementType.Waypoint,
                Waypoints = waypoints,
                Speed = speed,
                CurrentWaypointIndex = 0,
                WaypointWaitTime = waitTime,
                CurrentWaitTime = 0f,
                Direction = 1,
                Velocity = Vector2.Zero,
                PreviousPosition = firstPoint
            };
        }
    }
}
