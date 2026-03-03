using UnityEngine;
using WaveGame.Combat.Damage;

namespace WaveGame.Combat.Projectiles
{
    public sealed class HitResolver
    {
        private readonly IProjectileTargetProvider _targetProvider;

        public HitResolver(IProjectileTargetProvider targetProvider)
        {
            _targetProvider = targetProvider;
        }

        public void Resolve(in HitEvent hitEvent)
        {
            if (!_targetProvider.TryGetTarget(hitEvent.TargetId, out IDamageable target) || !target.IsAlive)
            {
                return;
            }

            var isCritical = hitEvent.CritCandidate && Random.value <= hitEvent.CritChance;
            var damage = hitEvent.BaseDamage * hitEvent.DamageScale;
            if (isCritical)
            {
                damage *= hitEvent.CritMultiplier;
            }

            target.ApplyDamage(damage, hitEvent.DamageType, hitEvent.OwnerId, isCritical, hitEvent.KnockbackForce, hitEvent.HitPoint, hitEvent.HitNormal);
        }
    }
}
