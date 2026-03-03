using UnityEngine;
using WaveGame.Combat.Projectiles;

namespace WaveGame.Combat.Damage
{
    public readonly struct DamageEvent
    {
        public readonly float Amount;
        public readonly DamageType DamageType;
        public readonly int OwnerId;
        public readonly bool IsCritical;
        public readonly float KnockbackForce;
        public readonly Vector3 HitPoint;
        public readonly Vector3 HitNormal;

        public DamageEvent(float amount, DamageType damageType, int ownerId, bool isCritical, float knockbackForce, Vector3 hitPoint, Vector3 hitNormal)
        {
            Amount = amount;
            DamageType = damageType;
            OwnerId = ownerId;
            IsCritical = isCritical;
            KnockbackForce = knockbackForce;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
        }
    }
}
