// Components/StunSettings.cs
namespace ECS_Example.Components
{
    public struct StunSettings
    {
        public float DamageStunDuration;

        public StunSettings(float damageStunDuration)
        {
            DamageStunDuration = damageStunDuration;
        }
    }
}