using Microsoft.Xna.Framework.Media;

namespace ECS_Base.Mechanics.Audio.Components
{
    /// <summary>
    /// Component for background music playback
    /// Supports crossfading between tracks
    /// Note: Only one music component should be active at a time (MonoGame MediaPlayer limitation)
    /// </summary>
    public struct MusicComponent
    {
        public Song CurrentSong;
        public Song NextSong; // For crossfade transitions
        public float Volume; // 0-1
        public float CrossfadeDuration; // Seconds for crossfade
        public float CrossfadeTimer; // Current crossfade progress
        public bool IsPlaying;
        public bool IsCrossfading;

        public MusicComponent(Song song)
        {
            CurrentSong = song;
            NextSong = null;
            Volume = 1.0f;
            CrossfadeDuration = 2.0f;
            CrossfadeTimer = 0f;
            IsPlaying = false;
            IsCrossfading = false;
        }

        /// <summary>
        /// Start crossfade to a new song
        /// </summary>
        public void CrossfadeTo(Song newSong, float duration = 2.0f)
        {
            NextSong = newSong;
            CrossfadeDuration = duration;
            CrossfadeTimer = 0f;
            IsCrossfading = true;
        }
    }
}
