using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Tweening.Components;
using ECS_Base.Mechanics.Tweening.Data;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Rendering.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Tweening.Systems
{
    /// <summary>
    /// System that updates tweens
    /// </summary>
    public class TweenSystem
    {
        /// <summary>
        /// Update all tweens
        /// </summary>
        public void Update(World world, float deltaTime)
        {
            var tweensToRemove = new List<Entity>();

            foreach (var entity in world.Query<TweenComponent>())
            {
                if (!world.TryGetComponent<TweenComponent>(entity, out var tween))
                    continue;

                // Update elapsed time
                tween.Elapsed += deltaTime;

                // Check if complete
                if (tween.Elapsed >= tween.Duration)
                {
                    if (tween.Loop)
                    {
                        tween.Elapsed -= tween.Duration;
                    }
                    else if (tween.PingPong)
                    {
                        tween.Elapsed -= tween.Duration;
                        tween.IsReversing = !tween.IsReversing;
                    }
                    else
                    {
                        tween.Elapsed = tween.Duration; // Clamp to duration
                        tween.OnComplete?.Invoke();

                        if (tween.RemoveOnComplete)
                        {
                            tweensToRemove.Add(entity);
                        }
                    }
                }

                // Apply tween to target
                ApplyTweenValue(world, entity, tween);

                world.AddComponent(entity, tween);
            }

            // Remove completed tweens
            foreach (var entity in tweensToRemove)
            {
                world.RemoveComponent<TweenComponent>(entity);
            }
        }

        private void ApplyTweenValue(World world, Entity entity, TweenComponent tween)
        {
            float value = tween.CurrentValue;

            switch (tween.Target)
            {
                case TweenTarget.PositionX:
                    if (world.TryGetComponent<PositionComponent>(entity, out var posX))
                    {
                        posX.Value = new Vector2(value, posX.Value.Y);
                        world.AddComponent(entity, posX);
                    }
                    break;

                case TweenTarget.PositionY:
                    if (world.TryGetComponent<PositionComponent>(entity, out var posY))
                    {
                        posY.Value = new Vector2(posY.Value.X, value);
                        world.AddComponent(entity, posY);
                    }
                    break;

                case TweenTarget.ScaleX:
                    if (world.TryGetComponent<SpriteComponent>(entity, out var scaleXSprite))
                    {
                        scaleXSprite.Scale = new Vector2(value, scaleXSprite.Scale.Y);
                        world.AddComponent(entity, scaleXSprite);
                    }
                    break;

                case TweenTarget.ScaleY:
                    if (world.TryGetComponent<SpriteComponent>(entity, out var scaleYSprite))
                    {
                        scaleYSprite.Scale = new Vector2(scaleYSprite.Scale.X, value);
                        world.AddComponent(entity, scaleYSprite);
                    }
                    break;

                case TweenTarget.Rotation:
                    if (world.TryGetComponent<SpriteComponent>(entity, out var sprite))
                    {
                        sprite.Rotation = value;
                        world.AddComponent(entity, sprite);
                    }
                    break;

                case TweenTarget.Alpha:
                    if (world.TryGetComponent<SpriteComponent>(entity, out var alphaSprite))
                    {
                        alphaSprite.Opacity = value;
                        world.AddComponent(entity, alphaSprite);
                    }
                    break;

                case TweenTarget.Custom:
                    // Custom tweens are handled by the user via CurrentValue property
                    break;
            }
        }

        /// <summary>
        /// Create a position tween
        /// </summary>
        public static TweenComponent TweenPosition(Vector2 start, Vector2 end, float duration, EasingType easing = EasingType.Linear)
        {
            // Note: This creates two separate tweens (X and Y)
            // In practice, you'd add both to the entity
            return new TweenComponent(TweenTarget.PositionX, start.X, end.X, duration, easing);
        }

        /// <summary>
        /// Create a scale tween
        /// </summary>
        public static TweenComponent TweenScale(float start, float end, float duration, EasingType easing = EasingType.Linear)
        {
            return new TweenComponent(TweenTarget.ScaleX, start, end, duration, easing);
        }

        /// <summary>
        /// Create a rotation tween
        /// </summary>
        public static TweenComponent TweenRotation(float start, float end, float duration, EasingType easing = EasingType.Linear)
        {
            return new TweenComponent(TweenTarget.Rotation, start, end, duration, easing);
        }

        /// <summary>
        /// Create an alpha/fade tween
        /// </summary>
        public static TweenComponent TweenAlpha(float start, float end, float duration, EasingType easing = EasingType.Linear)
        {
            return new TweenComponent(TweenTarget.Alpha, start, end, duration, easing);
        }
    }
}
