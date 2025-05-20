// Components/Input.cs
namespace ECS_Example.Components
{
    public struct Input
    {
        public bool IsPlayerControlled;

        public Input(bool isPlayerControlled)
        {
            IsPlayerControlled = isPlayerControlled;
        }
    }
}