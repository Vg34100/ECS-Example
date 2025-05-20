// Components/FlashEffect.cs
namespace ECS_Example.Components
{
    public struct FlashEffect
    {
        public float FlashInterval;
        public float TimeUntilFlash;
        public bool IsVisible;

        public FlashEffect(float flashInterval)
        {
            FlashInterval = flashInterval;
            TimeUntilFlash = flashInterval;
            IsVisible = true;
        }
    }
}