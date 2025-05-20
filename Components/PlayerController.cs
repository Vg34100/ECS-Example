// Components/PlayerController.cs
namespace ECS_Example.Components
{
    public struct PlayerController
    {
        public float MoveSpeed;

        public PlayerController(float moveSpeed)
        {
            MoveSpeed = moveSpeed;
        }
    }
}