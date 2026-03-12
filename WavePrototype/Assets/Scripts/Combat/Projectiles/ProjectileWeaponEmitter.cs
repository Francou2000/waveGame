using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    public sealed class ProjectileWeaponEmitter : MonoBehaviour
    {
        [SerializeField] private ProjectileSystem projectileSystem;
        [SerializeField] private ProjectileDefinition projectileDefinition;
        [SerializeField] private int ownerEntityId;
        [SerializeField] private int teamId;

        [Header("Aiming")]
        [SerializeField] private bool aimAtNearestTarget = true;
        [SerializeField, Min(0.1f)] private float acquireRadius = 18f;
        [SerializeField, Range(0f, 180f)] private float preferForwardAngle = 180f;

        public void Fire()
        {
            if (projectileSystem == null || projectileDefinition == null)
            {
                return;
            }

            var origin = transform.position;
            var direction = transform.forward;
            var targetId = -1;

            if (aimAtNearestTarget && projectileSystem.TargetProvider != null)
            {
                targetId = projectileSystem.TargetProvider.AcquireTarget(origin, direction, acquireRadius, preferForwardAngle, teamId);
                if (targetId >= 0 && projectileSystem.TargetProvider.TryGetTarget(targetId, out var target) && target != null && target.IsAlive)
                {
                    var toTarget = target.GetAimPoint() - origin;
                    toTarget.y = 0f;
                    if (toTarget.sqrMagnitude > 0.0001f)
                    {
                        direction = toTarget.normalized;
                    }
                }
            }

            var spawn = new ProjectileSpawnContext(
                projectileDefinition,
                ownerEntityId,
                teamId,
                origin,
                direction,
                targetId);

            projectileSystem.TrySpawn(spawn);
        }
    }
}
