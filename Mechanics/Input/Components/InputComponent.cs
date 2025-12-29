using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Input.Components
{
    public struct InputComponent
    {
        public Vector2 Movement;
        public bool Jump;
        public bool JumpHeld;

        public InputComponent()
        {
            Movement = Vector2.Zero;
            Jump = false;
            JumpHeld = false;
        }
    }
}
