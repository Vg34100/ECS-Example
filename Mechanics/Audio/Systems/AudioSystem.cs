using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Audio.Components;
using ECS_Base.Mechanics.Movement.Components;
using ECS_Base.Mechanics.Camera.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ECS_Base.Mechanics.Audio.Systems
{
    /// <summary>
    /// Handles sound effect playback
    /// Supports 3D positional audio based on distance to camera/listener
    /// </summary>
    public class AudioSystem
    {
        private List<Entity> _entitiesToRemove = new List<Entity>();

        public void Update(World world, float deltaTime)
        {
            _entitiesToRemove.Clear();

            // Find camera position for 3D audio
            Vector2 listenerPosition = Vector2.Zero;
            var cameraEntity = world.GetEntities()
                .FirstOrDefault(e => world.TryGetComponent<CameraComponent>(e, out var cam) && cam.IsActive);

            if (cameraEntity != null && world.TryGetComponent<CameraComponent>(cameraEntity, out var camera))
            {
                listenerPosition = camera.Position;
            }

            foreach (var entity in world.GetEntities())
            {
                if (!world.TryGetComponent<AudioComponent>(entity, out var audio))
                    continue;

                if (audio.Sound == null)
                    continue;

                // Play sound if triggered
                if (audio.ShouldPlay)
                {
                    float finalVolume = audio.Volume;

                    // Calculate 3D positional volume
                    if (audio.Is3D && world.TryGetComponent<PositionComponent>(entity, out var position))
                    {
                        float distance = Vector2.Distance(position.Value, listenerPosition);
                        float maxDistance = 500f; // Sounds inaudible beyond this distance
                        float volumeFalloff = 1.0f - MathHelper.Clamp(distance / maxDistance, 0f, 1f);
                        finalVolume *= volumeFalloff;
                    }

                    if (audio.IsLooping)
                    {
                        // Create instance for looping sounds
                        if (audio.Instance == null)
                        {
                            audio.Instance = audio.Sound.CreateInstance();
                            audio.Instance.IsLooped = true;
                        }

                        audio.Instance.Volume = finalVolume;
                        audio.Instance.Pitch = audio.Pitch;
                        audio.Instance.Pan = audio.Pan;
                        audio.Instance.Play();
                    }
                    else
                    {
                        // Play one-shot sound
                        audio.Sound.Play(finalVolume, audio.Pitch, audio.Pan);

                        // Mark for removal if PlayOnce is true
                        if (audio.PlayOnce)
                        {
                            _entitiesToRemove.Add(entity);
                        }
                    }

                    audio.ShouldPlay = false;
                    world.AddComponent(entity, audio);
                }
                else if (audio.IsLooping && audio.Instance != null)
                {
                    // Update volume for looping sounds (for 3D audio)
                    float finalVolume = audio.Volume;

                    if (audio.Is3D && world.TryGetComponent<PositionComponent>(entity, out var position))
                    {
                        float distance = Vector2.Distance(position.Value, listenerPosition);
                        float maxDistance = 500f;
                        float volumeFalloff = 1.0f - MathHelper.Clamp(distance / maxDistance, 0f, 1f);
                        finalVolume *= volumeFalloff;
                    }

                    audio.Instance.Volume = finalVolume;
                }
            }

            // Remove play-once audio components
            foreach (var entity in _entitiesToRemove)
            {
                world.RemoveComponent<AudioComponent>(entity);
            }
        }

        /// <summary>
        /// Stop all currently playing looping sounds
        /// </summary>
        public void StopAllLoopingSounds(World world)
        {
            foreach (var entity in world.GetEntities())
            {
                if (world.TryGetComponent<AudioComponent>(entity, out var audio))
                {
                    if (audio.Instance != null)
                    {
                        audio.Instance.Stop();
                    }
                }
            }
        }
    }
}
