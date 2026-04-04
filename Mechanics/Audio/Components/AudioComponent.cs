using Microsoft.Xna.Framework.Audio;

namespace ECS_Base.Mechanics.Audio.Components
{
    /// <summary>
    /// Component for playing sound effects
    /// Supports one-shot and looping sounds, 3D positional audio, volume/pitch/pan control
    /// </summary>
    public struct AudioComponent
    {
        public SoundEffect Sound;
        public float Volume; // 0-1
        public float Pitch; // -1 to 1 (lower to higher pitch)
        public float Pan; // -1 (left) to 1 (right)
        public bool IsLooping;
        public bool PlayOnce; // Auto-remove component after playing
        public bool Is3D; // Position-based volume attenuation
        public bool ShouldPlay; // Trigger to play sound
        public SoundEffectInstance Instance; // For looping sounds

        public AudioComponent(SoundEffect sound, bool loop = false, bool playOnce = true)
        {
            Sound = sound;
            Volume = 1.0f;
            Pitch = 0f;
            Pan = 0f;
            IsLooping = loop;
            PlayOnce = playOnce;
            Is3D = false;
            ShouldPlay = true;
            Instance = null;
        }
    }
}
