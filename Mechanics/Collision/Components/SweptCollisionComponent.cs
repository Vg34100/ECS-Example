namespace ECS_Base.Mechanics.Collision.Components
{
    /// <summary>
    /// Marker component - entities with this use swept collision detection instead of basic AABB
    /// Swept collision prevents tunneling at high speeds by checking the path between frames
    /// Use this for: fast projectiles, dashes, high-speed movement
    /// </summary>
    public struct SweptCollisionComponent
    {
        // This is just a marker - no data needed
        // Having this component means "use swept collision for this entity"
    }
}
