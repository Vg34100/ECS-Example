namespace ECS_Base.Mechanics.Core
{
    /// <summary>
    /// Marker for systems that should update only in editor mode.
    /// </summary>
    public interface IEditorSystem
    {
    }

    /// <summary>
    /// Marker for systems that should always update, even in editor mode.
    /// </summary>
    public interface IAlwaysUpdateSystem
    {
    }
}
