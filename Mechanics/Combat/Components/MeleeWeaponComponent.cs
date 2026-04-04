using Microsoft.Xna.Framework;

namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Component for melee weapons
    /// </summary>
    public struct MeleeWeaponComponent
    {
        /// <summary>Base damage dealt</summary>
        public float Damage;

        /// <summary>Attack range in pixels</summary>
        public float Range;

        /// <summary>Time between attacks in seconds</summary>
        public float AttackSpeed;

        /// <summary>Duration of attack animation in seconds</summary>
        public float AttackDuration;

        /// <summary>Knockback force applied on hit</summary>
        public float Knockback;

        /// <summary>Attack arc angle in radians (e.g., PI/4 = 45 degrees)</summary>
        public float AttackArc;

        /// <summary>Time since last attack</summary>
        public float TimeSinceLastAttack;

        /// <summary>Can this weapon attack right now?</summary>
        public readonly bool CanAttack => TimeSinceLastAttack >= AttackSpeed;

        public MeleeWeaponComponent(float damage, float range, float attackSpeed = 0.5f)
        {
            Damage = damage;
            Range = range;
            AttackSpeed = attackSpeed;
            AttackDuration = 0.3f;
            Knockback = 100f;
            AttackArc = MathHelper.PiOver2; // 90 degrees
            TimeSinceLastAttack = attackSpeed; // Start ready to attack
        }
    }
}
