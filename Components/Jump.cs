// Components/Jump.cs
namespace ECS_Example.Components
{
    public struct Jump
    {
        public float JumpVelocity;
        public bool CanJump;

        public Jump(float jumpVelocity)
        {
            JumpVelocity = jumpVelocity;
            CanJump = true;
        }
    }
}