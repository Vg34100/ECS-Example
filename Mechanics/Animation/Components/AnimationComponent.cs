using ECS_Base.Mechanics.Animation.Data;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Animation.Components
{
    /// <summary>
    /// Component for sprite sheet animation
    /// Works with SpriteComponent to update the source rectangle
    /// </summary>
    public struct AnimationComponent
    {
        public Dictionary<string, AnimationData> Animations; // Library of animations for this entity
        public string CurrentAnimation; // Name of currently playing animation
        public int CurrentFrame; // Current frame index
        public float FrameTimer; // Time accumulated for current frame
        public bool IsLooping; // Whether current animation loops
        public float SpeedMultiplier; // Speed modifier (1.0 = normal, 2.0 = 2x speed)
        public bool IsPlaying; // Whether animation is currently playing

        public AnimationComponent(Dictionary<string, AnimationData> animations)
        {
            Animations = animations;
            CurrentAnimation = "";
            CurrentFrame = 0;
            FrameTimer = 0f;
            IsLooping = true;
            SpeedMultiplier = 1.0f;
            IsPlaying = false;
        }

        /// <summary>
        /// Play an animation by name
        /// </summary>
        public void Play(string animationName, bool loop = true)
        {
            if (Animations.ContainsKey(animationName))
            {
                // Only restart if switching to a different animation
                if (CurrentAnimation != animationName)
                {
                    CurrentAnimation = animationName;
                    CurrentFrame = 0;
                    FrameTimer = 0f;
                    IsLooping = loop;
                }
                IsPlaying = true;
            }
        }

        /// <summary>
        /// Stop the current animation
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
        }

        /// <summary>
        /// Reset current animation to first frame
        /// </summary>
        public void Reset()
        {
            CurrentFrame = 0;
            FrameTimer = 0f;
        }

        /// <summary>
        /// Get the current animation data
        /// </summary>
        public AnimationData GetCurrentAnimationData()
        {
            if (!string.IsNullOrEmpty(CurrentAnimation) && Animations.ContainsKey(CurrentAnimation))
            {
                return Animations[CurrentAnimation];
            }
            return null;
        }
    }
}
