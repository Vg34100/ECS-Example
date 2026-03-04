using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Animation.Data
{
    /// <summary>
    /// Defines a single animation frame
    /// </summary>
    public class AnimationFrame
    {
        public Rectangle SourceRect; // Position on sprite sheet
        public float Duration; // How long to display this frame (seconds)

        public AnimationFrame(Rectangle sourceRect, float duration)
        {
            SourceRect = sourceRect;
            Duration = duration;
        }
    }

    /// <summary>
    /// Defines an animation clip (sequence of frames)
    /// Can be shared across multiple entities
    /// </summary>
    public class AnimationData
    {
        public string Name;
        public List<AnimationFrame> Frames;
        public bool Loop;

        public AnimationData(string name, bool loop = true)
        {
            Name = name;
            Loop = loop;
            Frames = new List<AnimationFrame>();
        }

        /// <summary>
        /// Add a frame to this animation
        /// </summary>
        public void AddFrame(Rectangle sourceRect, float duration)
        {
            Frames.Add(new AnimationFrame(sourceRect, duration));
        }

        /// <summary>
        /// Get total duration of the animation
        /// </summary>
        public float GetTotalDuration()
        {
            float total = 0f;
            foreach (var frame in Frames)
            {
                total += frame.Duration;
            }
            return total;
        }
    }
}
