// Components/Stunned.cs
namespace ECS_Example.Components
{
    public struct Stunned
    {
        public float Duration;
        public float TimeRemaining;

        public Stunned(float duration)
        {
            Duration = duration;
            TimeRemaining = duration;
        }
    }
}