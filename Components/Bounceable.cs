// Components/Bounceable.cs
namespace ECS_Example.Components
{
    public struct Bounceable
    {
        public float BounceVelocity;

        public Bounceable(float bounceVelocity)
        {
            BounceVelocity = bounceVelocity;
        }
    }
}