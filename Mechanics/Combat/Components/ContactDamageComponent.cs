namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Deals damage on contact.
    /// </summary>
    public struct ContactDamageComponent
    {
        public float Damage;

        public ContactDamageComponent(float damage)
        {
            Damage = damage;
        }
    }
}
