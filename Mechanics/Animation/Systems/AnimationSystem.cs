using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Animation.Components;
using ECS_Base.Mechanics.Rendering.Components;
using System;

namespace ECS_Base.Mechanics.Animation.Systems
{
    /// <summary>
    /// Updates sprite animations
    /// Advances frames based on time and updates SpriteComponent's source rectangle
    /// </summary>
    public class AnimationSystem
    {
        public void Update(World world, float deltaTime)
        {
            foreach (var entity in world.GetEntities())
            {
                if (!world.TryGetComponent<AnimationComponent>(entity, out var animation))
                    continue;

                if (!world.TryGetComponent<SpriteComponent>(entity, out var sprite))
                    continue;

                // Skip if not playing
                if (!animation.IsPlaying)
                    continue;

                var animData = animation.GetCurrentAnimationData();
                if (animData == null || animData.Frames.Count == 0)
                    continue;

                // Update frame timer
                animation.FrameTimer += deltaTime * animation.SpeedMultiplier;

                // Get current frame duration
                var currentFrame = animData.Frames[animation.CurrentFrame];

                // Check if we need to advance to next frame
                if (animation.FrameTimer >= currentFrame.Duration)
                {
                    animation.FrameTimer -= currentFrame.Duration;
                    animation.CurrentFrame++;

                    // Handle end of animation
                    if (animation.CurrentFrame >= animData.Frames.Count)
                    {
                        if (animation.IsLooping)
                        {
                            // Loop back to first frame
                            animation.CurrentFrame = 0;
                        }
                        else
                        {
                            // Stop on last frame
                            animation.CurrentFrame = animData.Frames.Count - 1;
                            animation.IsPlaying = false;

                            // Fire animation complete event (future: use event system)
                            Console.WriteLine($"Animation '{animation.CurrentAnimation}' completed");
                        }
                    }
                }

                // Update sprite's source rectangle to current frame
                var frameData = animData.Frames[animation.CurrentFrame];
                sprite.SourceRectangle = frameData.SourceRect;

                // Update components
                world.AddComponent(entity, animation);
                world.AddComponent(entity, sprite);
            }
        }
    }
}
