using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ECS_Base.Mechanics.Combat.Components
{
    /// <summary>
    /// Active melee attack state
    /// </summary>
    public struct MeleeAttackComponent
    {
        /// <summary>Entity that initiated this attack</summary>
        public int AttackerEntityId;

        /// <summary>Attack damage</summary>
        public float Damage;

        /// <summary>Attack direction (normalized)</summary>
        public Vector2 Direction;

        /// <summary>Attack range</summary>
        public float Range;

        /// <summary>Attack arc angle</summary>
        public float Arc;

        /// <summary>Time elapsed since attack started</summary>
        public float TimeElapsed;

        /// <summary>Total attack duration</summary>
        public float Duration;

        /// <summary>Knockback force</summary>
        public float Knockback;

        /// <summary>Entities that have already been hit by this attack</summary>
        public HashSet<int> AlreadyHit;

        /// <summary>Is this attack still active?</summary>
        public readonly bool IsActive => TimeElapsed < Duration;

        public MeleeAttackComponent(int attackerId, float damage, Vector2 direction, float range, float arc, float duration, float knockback)
        {
            AttackerEntityId = attackerId;
            Damage = damage;
            Direction = direction;
            Range = range;
            Arc = arc;
            Duration = duration;
            Knockback = knockback;
            TimeElapsed = 0f;
            AlreadyHit = new HashSet<int>();
        }
    }
}
