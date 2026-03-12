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

        public bool Resolve(in HitEvent hitEvent, out float resolvedDamage, out bool isCritical)
        {
            resolvedDamage = 0f;
            isCritical = false;

            if (!_targetProvider.TryGetTarget(hitEvent.TargetId, out IDamageable target) || !target.IsAlive)
            {
                return false;
            }

            isCritical = hitEvent.CritCandidate && Random.value <= hitEvent.CritChance;
            resolvedDamage = hitEvent.BaseDamage * hitEvent.DamageScale;
            if (isCritical)
            {
                resolvedDamage *= hitEvent.CritMultiplier;
            }

            target.ApplyDamage(new DamageEvent(resolvedDamage, hitEvent.DamageType, hitEvent.OwnerId, isCritical, hitEvent.KnockbackForce, hitEvent.HitPoint, hitEvent.HitNormal));
            return true;
        }
    }
}
