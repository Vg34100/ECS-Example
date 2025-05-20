// Components/Attack.cs
using Microsoft.Xna.Framework;

namespace ECS_Example.Components
{
    public struct Attack
    {
        public float Cooldown;
        public float TimeUntilNextAttack;
        public int Damage;
        public Vector2 HitboxSize;
        public float HitboxOffsetX;
        public bool IsFacingRight;
        public bool IsAttacking;
        public float AttackDuration; // How long the attack lasts
        public float AttackTimer;    // Time remaining in current attack

        public Attack(int damage, Vector2 hitboxSize, float hitboxOffsetX, float cooldown, float duration = 0.1f)
        {
            Damage = damage;
            HitboxSize = hitboxSize;
            HitboxOffsetX = hitboxOffsetX;
            Cooldown = cooldown;
            TimeUntilNextAttack = 0;
            IsFacingRight = true;
            IsAttacking = false;
            AttackDuration = duration;
            AttackTimer = 0;
        }
    }
}