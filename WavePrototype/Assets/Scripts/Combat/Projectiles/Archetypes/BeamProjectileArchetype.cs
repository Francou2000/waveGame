using UnityEngine;
using WaveGame.Combat.Damage;

namespace WaveGame.Combat.Projectiles.Archetypes
{
    public sealed class BeamProjectileArchetype : IProjectileArchetype
    {
        private readonly RaycastHit[] _hits;

        public BeamProjectileArchetype(int hitBufferSize)
        {
            _hits = new RaycastHit[hitBufferSize];
        }

        public void Simulate(ref ProjectileInstance projectile, float dt, IProjectileContext context)
        {
            if (context.TimeNow < projectile.NextTickTime)
            {
                return;
            }

            projectile.NextTickTime = context.TimeNow + Mathf.Max(0.01f, projectile.Definition.TickInterval);
            var count = context.SphereCastNonAlloc(
                projectile.Position,
                projectile.Definition.Radius,
                projectile.Direction,
                _hits,
                projectile.Definition.MaxDistance,
                projectile.Definition.HitMask);

            for (var i = 0; i < count; i++)
            {
                var hit = _hits[i];
                if (!hit.collider.GetComponentInParent<IDamageable>(out var damageable) || damageable.TeamId == projectile.TeamId)
                {
                    continue;
                }

                context.RegisterTarget(damageable);

                if (!context.HitRegistry.CanHit(projectile.InstanceId, damageable.EntityId, context.TimeNow, projectile.Definition.HitCooldownPerTarget))
                {
                    continue;
                }

                context.EnqueueHit(new HitEvent
                {
                    ProjectileId = projectile.InstanceId,
                    OwnerId = projectile.OwnerEntityId,
                    TargetId = damageable.EntityId,
                    HitPoint = hit.point,
                    HitNormal = hit.normal,
                    BaseDamage = projectile.Definition.BaseDamage,
                    DamageScale = projectile.DamageScale,
                    DamageType = projectile.Definition.DamageType,
                    CritCandidate = false,
                    CritChance = projectile.Definition.CritChance,
                    CritMultiplier = projectile.Definition.CritMultiplier,
                    KnockbackForce = projectile.Definition.KnockbackForce
                });
            }
        }
    }
}
