// Components/CameraTarget.cs
namespace ECS_Example.Components
{
    public struct CameraTarget
    {
        public bool IsActive;

        public CameraTarget(bool isActive = true)
        {
            IsActive = isActive;
        }
    }
}