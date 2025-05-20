// Components/Patrol.cs - Updated for platformer-style patrol
namespace ECS_Example.Components
{
    public struct Patrol
    {
        public float Speed;
        public bool FacingRight;
        public bool AvoidFalling; // true = Koopa style, false = Goomba style

        public Patrol(float speed, bool avoidFalling = false)
        {
            Speed = speed;
            FacingRight = false; // Start facing left
            AvoidFalling = avoidFalling;
        }
    }
}