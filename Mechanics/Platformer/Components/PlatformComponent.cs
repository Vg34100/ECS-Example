using ECS_Base.Mechanics.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Platformer.Components
{
    /// <summary>
    /// Component for platform entities
    /// </summary>
    public struct PlatformComponent
    {
        /// <summary>Width of the platform</summary>
        public float Width;

        /// <summary>Height of the platform</summary>
        public float Height;

        /// <summary>Is this a one-way platform? (can jump through from below)</summary>
        public bool OneWay;

        /// <summary>Offset from top to consider "on platform"</summary>
        public float SurfaceThreshold;

        /// <summary>Entities currently riding this platform</summary>
        public List<Entity> Riders;

        public PlatformComponent(float width, float height, bool oneWay = false)
        {
            Width = width;
            Height = height;
            OneWay = oneWay;
            SurfaceThreshold = 4f;
            Riders = new List<Entity>();
        }
    }
}
