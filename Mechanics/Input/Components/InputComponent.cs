using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Input.Components
{
    public struct InputComponent
    {
        public Vector2 Movement;
        public bool Jump;
        public bool JumpHeld;
        public bool Attack;
        public bool BlockHeld;
        public bool ShootHeld;
        public bool Bomb;
        public bool Block;

        public InputComponent()
        {
            Movement = Vector2.Zero;
            Jump = false;
            JumpHeld = false;
            Attack = false;
            BlockHeld = false;
            ShootHeld = false;
            Bomb = false;
            Block = false;
        }
    }
}
