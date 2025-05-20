// Components/Damager.cs
namespace ECS_Example.Components
{
    public enum DamageType
    {
        Contact,    // Damages on any contact
        FromAbove,  // Only damages when hit from above (like jumping on enemies)
        None        // Doesn't deal damage
    }

    public struct Damager
    {
        public int Damage;
        public DamageType Type;

        public Damager(int damage, DamageType type = DamageType.Contact)
        {
            Damage = damage;
            Type = type;
        }
    }
}