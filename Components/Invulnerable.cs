// Components/Invulnerable.cs
namespace ECS_Example.Components
{
    public struct Invulnerable
    {
        public float Duration;
        public float TimeRemaining;

        public Invulnerable(float duration)
        {
            Duration = duration;
            TimeRemaining = duration;
        }
    }
}