// Components/Gravity.cs
namespace ECS_Example.Components
{
    public struct Gravity
    {
        public float Value;
        public bool IsGrounded;

        public Gravity(float value)
        {
            Value = value;
            IsGrounded = false;
        }
    }
}