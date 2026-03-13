using UnityEngine;
using WaveGame.Combat.Damage;

namespace WaveGame.Combat.Projectiles.Archetypes
{
    public sealed class AoEProjectileArchetype : IProjectileArchetype
    {
        private readonly Collider[] _colliders;

        public AoEProjectileArchetype(int hitBufferSize)
        {
            _colliders = new Collider[hitBufferSize];
        }

        public void Simulate(ref ProjectileInstance projectile, float dt, IProjectileContext context)
        {
            var count = context.OverlapSphereNonAlloc(projectile.Position, projectile.Definition.Radius, _colliders, projectile.Definition.HitMask);
            for (var i = 0; i < count; i++)
            {
                var col = _colliders[i];
                var damageable = col != null ? col.GetComponentInParent<IDamageable>() : null;
                if (damageable == null || damageable.TeamId == projectile.TeamId)
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
                    HitPoint = damageable.Position,
                    HitNormal = Vector3.up,
                    BaseDamage = projectile.Definition.BaseDamage,
                    DamageScale = projectile.DamageScale,
                    DamageType = projectile.Definition.DamageType,
                    CritCandidate = true,
                    CritChance = projectile.Definition.CritChance,
                    CritMultiplier = projectile.Definition.CritMultiplier,
                    KnockbackForce = projectile.Definition.KnockbackForce
                });
            }

            projectile.IsFinished = true;
        }
    }
}
