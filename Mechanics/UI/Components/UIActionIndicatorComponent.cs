namespace ECS_Base.Mechanics.UI.Components
{
    /// <summary>
    /// Toggles UI icon color based on player action state.
    /// </summary>
    public struct UIActionIndicatorComponent
    {
        public int TargetEntityId;
        public ActionType Action;
        public Microsoft.Xna.Framework.Color OnColor;
        public Microsoft.Xna.Framework.Color OffColor;

        public UIActionIndicatorComponent(int targetEntityId, ActionType action, Microsoft.Xna.Framework.Color onColor, Microsoft.Xna.Framework.Color offColor)
        {
            TargetEntityId = targetEntityId;
            Action = action;
            OnColor = onColor;
            OffColor = offColor;
        }
    }

    public enum ActionType
    {
        Sword,
        Shield,
        Bow
    }
}
