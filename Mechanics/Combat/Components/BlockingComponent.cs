namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Marker for entities currently blocking.
    /// </summary>
    public struct BlockingComponent
    {
        public bool IsBlocking;

        public BlockingComponent(bool isBlocking)
        {
            IsBlocking = isBlocking;
        }
    }
}
