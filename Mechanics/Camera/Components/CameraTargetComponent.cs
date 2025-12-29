namespace ECS_Base.Mechanics.Camera.Components
{
    public struct CameraTargetComponent
    {
        public bool IsActive;

        public CameraTargetComponent(bool isActive = true)
        {
            IsActive = isActive;
        }
    }
}
