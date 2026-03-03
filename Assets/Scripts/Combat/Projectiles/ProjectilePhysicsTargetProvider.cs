using System.Collections.Generic;
using UnityEngine;
using WaveGame.Combat.Damage;

namespace WaveGame.Combat.Projectiles
{
    public sealed class ProjectilePhysicsTargetProvider : IProjectileTargetProvider
    {
        private readonly Dictionary<int, IDamageable> _targetsById = new(1024);
        private readonly Collider[] _buffer;
        private readonly LayerMask _enemyMask;

        public ProjectilePhysicsTargetProvider(int bufferSize, LayerMask enemyMask)
        {
            _buffer = new Collider[bufferSize];
            _enemyMask = enemyMask;
        }

        public void Register(IDamageable target)
        {
            if (target == null)
            {
                return;
            }

            _targetsById[target.EntityId] = target;
        }

        public bool TryGetTarget(int entityId, out IDamageable target)
        {
            return _targetsById.TryGetValue(entityId, out target);
        }

        public int AcquireTarget(Vector3 origin, Vector3 forward, float radius, float preferForwardAngleDeg, int teamId)
        {
            var count = Physics.OverlapSphereNonAlloc(origin, radius, _buffer, _enemyMask, QueryTriggerInteraction.Collide);
            var bestScore = float.NegativeInfinity;
            var bestId = -1;

            for (var i = 0; i < count; i++)
            {
                var col = _buffer[i];
                if (col == null || !col.TryGetComponent<IDamageable>(out var damageable) || !damageable.IsAlive)
                {
                    continue;
                }

                if (damageable.TeamId == teamId)
                {
                    continue;
                }

                Register(damageable);

                var to = damageable.Position - origin;
                var distScore = -to.sqrMagnitude;
                var angle = Vector3.Angle(forward, to.normalized);
                var angleScore = angle <= preferForwardAngleDeg ? 1f : 0f;
                var score = distScore + (angleScore * 1000f);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestId = damageable.EntityId;
                }
            }

            return bestId;
        }
    }
}
