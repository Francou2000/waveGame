using UnityEngine;
using WaveGame.Combat.Damage;

namespace WaveGame.Combat.Projectiles.Archetypes
{
    public sealed class StraightProjectileArchetype : IProjectileArchetype
    {
        private readonly RaycastHit[] _hits;

        public StraightProjectileArchetype(int hitBufferSize)
        {
            _hits = new RaycastHit[hitBufferSize];
        }

        public void Simulate(ref ProjectileInstance projectile, float dt, IProjectileContext context)
        {
            var definition = projectile.Definition;
            var previous = projectile.Position;

            projectile.Speed += definition.Acceleration * dt;
            projectile.Speed *= (1f - (definition.Drag * dt));

            var displacement = projectile.Direction * projectile.Speed * dt;
            var next = previous + displacement;
            var distance = displacement.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return;
            }

            var hitCount = context.SphereCastNonAlloc(previous, definition.Radius, projectile.Direction, _hits, distance, definition.HitMask);
            var closestHitIndex = GetClosestHitIndex(hitCount);
            if (closestHitIndex < 0)
            {
                projectile.Position = next;
                projectile.Travelled += distance;
                return;
            }

            var hit = _hits[closestHitIndex];
            projectile.Position = hit.point;
            projectile.Travelled += hit.distance;
            HandleHit(ref projectile, context, hit);
        }

        private int GetClosestHitIndex(int hitCount)
        {
            if (hitCount <= 0)
            {
                return -1;
            }

            var bestIndex = -1;
            var bestDistance = float.MaxValue;
            for (var i = 0; i < hitCount; i++)
            {
                if (_hits[i].distance < bestDistance)
                {
                    bestDistance = _hits[i].distance;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private void HandleHit(ref ProjectileInstance projectile, IProjectileContext context, in RaycastHit hit)
        {
            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.TeamId != projectile.TeamId)
            {
                context.RegisterTarget(damageable);

                if (context.HitRegistry.CanHit(projectile.InstanceId, damageable.EntityId, context.TimeNow, projectile.Definition.HitCooldownPerTarget))
                {
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
                        CritCandidate = true,
                        CritChance = projectile.Definition.CritChance,
                        CritMultiplier = projectile.Definition.CritMultiplier,
                        KnockbackForce = projectile.Definition.KnockbackForce
                    });
                }

                projectile.HitsDone++;
                if (projectile.PiercesLeft > 0)
                {
                    projectile.PiercesLeft--;
                    projectile.DamageScale *= (1f - projectile.Definition.FalloffPerPierce);
                    return;
                }

                if (projectile.Definition.StopOnEnemy || projectile.HitsDone >= projectile.Definition.MaxHitsTotal)
                {
                    projectile.IsFinished = true;
                }

                return;
            }

            if (projectile.Definition.StopOnWorld)
            {
                projectile.IsFinished = true;
            }
        }
    }
}
