namespace ECS_Base.Mechanics.Movement.Components
{
    /// <summary>
    /// Simple back-and-forth patrol on the X axis.
    /// </summary>
    public struct PatrolComponent
    {
        public float MinX;
        public float MaxX;
        public float Speed;
        public int Direction; // -1 left, +1 right

        public PatrolComponent(float minX, float maxX, float speed = 40f, int direction = -1)
        {
            MinX = minX;
            MaxX = maxX;
            Speed = speed;
            Direction = direction;
        }
    }
}
