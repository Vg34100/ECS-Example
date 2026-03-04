using ECS_Base.Mechanics.Core;
using ECS_Base.Mechanics.Audio.Components;
using Microsoft.Xna.Framework.Media;
using System.Linq;

namespace ECS_Base.Mechanics.Audio.Systems
{
    /// <summary>
    /// Handles background music playback and crossfading
    /// Note: MonoGame's MediaPlayer can only play one song at a time
    /// </summary>
    public class MusicSystem
    {
        public void Update(World world, float deltaTime)
        {
            // Find the active music component (should only be one)
            var musicEntity = world.GetEntities()
                .FirstOrDefault(e => world.HasComponent<MusicComponent>(e));

            if (musicEntity == null)
                return;

            if (!world.TryGetComponent<MusicComponent>(musicEntity, out var music))
                return;

            // Handle crossfading
            if (music.IsCrossfading && music.NextSong != null)
            {
                music.CrossfadeTimer += deltaTime;
                float progress = music.CrossfadeTimer / music.CrossfadeDuration;

                if (progress >= 1.0f)
                {
                    // Crossfade complete - switch to new song
                    MediaPlayer.Stop();
                    music.CurrentSong = music.NextSong;
                    music.NextSong = null;
                    music.IsCrossfading = false;
                    music.CrossfadeTimer = 0f;

                    if (music.IsPlaying)
                    {
                        MediaPlayer.Play(music.CurrentSong);
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = music.Volume;
                    }
                }
                else
                {
                    // Fade out current song
                    float fadeVolume = (1.0f - progress) * music.Volume;
                    MediaPlayer.Volume = fadeVolume;
                }

                world.AddComponent(musicEntity, music);
                return;
            }

            // Normal playback
            if (music.IsPlaying)
            {
                if (MediaPlayer.State != MediaState.Playing)
                {
                    if (music.CurrentSong != null)
                    {
                        MediaPlayer.Play(music.CurrentSong);
                        MediaPlayer.IsRepeating = true;
                        MediaPlayer.Volume = music.Volume;
                    }
                }
                else
                {
                    // Update volume
                    MediaPlayer.Volume = music.Volume;
                }
            }
            else
            {
                // Stop if not playing
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Stop();
                }
            }
        }

        /// <summary>
        /// Stop music playback
        /// </summary>
        public void Stop()
        {
            MediaPlayer.Stop();
        }
    }
}
