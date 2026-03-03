using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    /// <summary>
    /// Emisor survivors-like: dispara automáticamente con cooldown por timestamp,
    /// usa auto-aim y puede lanzar múltiples proyectiles con spread y burst.
    /// </summary>
    public sealed class AutoFireWeaponEmitter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProjectileSystem projectileSystem;
        [SerializeField] private WeaponDefinition weaponDefinition;

        [Header("Owner")]
        [SerializeField] private int ownerEntityId = 1;
        [SerializeField] private int teamId = 1;

        [Header("Stats/Powerups")]
        [SerializeField] private float attackSpeedMultiplier = 1f;
        [SerializeField] private int additionalProjectiles;
        [SerializeField] private float additionalSpread;

        [Header("Testing")]
        [SerializeField] private bool autoFireEnabled = true;
        [SerializeField] private bool requireMouseHold;

        private int _currentTargetId = -1;
        private float _nextRetargetTime;
        private float _nextFireTime;
        private int _burstShotsRemaining;
        private float _nextBurstShotTime;

        private void Update()
        {
            if (!CanFireNow(Time.time))
            {
                return;
            }

            if (_burstShotsRemaining > 0)
            {
                FireVolley(Time.time);
                _burstShotsRemaining--;
                _nextBurstShotTime = Time.time + Mathf.Max(0f, weaponDefinition.BurstInterval);
                return;
            }

            StartBurst(Time.time);
        }

        private bool CanFireNow(float now)
        {
            if (!autoFireEnabled || projectileSystem == null || weaponDefinition == null || weaponDefinition.ProjectileDefinition == null)
            {
                return false;
            }

            if (requireMouseHold && !Input.GetMouseButton(0))
            {
                return false;
            }

            if (_burstShotsRemaining > 0)
            {
                return now >= _nextBurstShotTime;
            }

            return now >= _nextFireTime;
        }

        private void StartBurst(float now)
        {
            _burstShotsRemaining = Mathf.Max(1, weaponDefinition.BurstCount);
            _nextFireTime = now + GetFinalCooldown();
        }

        private float GetFinalCooldown()
        {
            var speed = Mathf.Max(0.01f, attackSpeedMultiplier);
            var cooldown = weaponDefinition.BaseCooldown / speed;
            return Mathf.Max(0.05f, cooldown);
        }

        private void FireVolley(float now)
        {
            var muzzlePosition = transform.TransformPoint(weaponDefinition.MuzzleOffset);
            var targetId = AcquireTarget(muzzlePosition, now);
            var aimDirection = ResolveAimDirection(muzzlePosition, targetId);

            var projectilesPerShot = Mathf.Max(1, weaponDefinition.ProjectilesPerShot + additionalProjectiles);
            var spreadAngle = Mathf.Max(0f, weaponDefinition.SpreadAngle + additionalSpread);
            for (var i = 0; i < projectilesPerShot; i++)
            {
                var direction = ApplySpread(aimDirection, spreadAngle, i, projectilesPerShot);
                var spawn = new ProjectileSpawnContext(
                    weaponDefinition.ProjectileDefinition,
                    ownerEntityId,
                    teamId,
                    muzzlePosition,
                    direction,
                    targetId);

                projectileSystem.TrySpawn(spawn);
            }
        }

        private int AcquireTarget(Vector3 origin, float now)
        {
            if (now >= _nextRetargetTime || !IsTargetValid(_currentTargetId))
            {
                var cone = weaponDefinition.TargetingMode == TargetingMode.ForwardCone ? weaponDefinition.ConeAngle : 180f;
                var forward = weaponDefinition.TargetingMode == TargetingMode.RandomInRange ? GetRandomPlanarDirection() : transform.forward;

                _currentTargetId = projectileSystem.TargetProvider.AcquireTarget(
                    origin,
                    forward,
                    weaponDefinition.Range,
                    cone,
                    teamId);

                _nextRetargetTime = now + Mathf.Max(0.05f, weaponDefinition.RetargetInterval);
            }

            if (weaponDefinition.RequiresLineOfSight && _currentTargetId >= 0)
            {
                if (!HasLineOfSight(origin, _currentTargetId))
                {
                    _currentTargetId = -1;
                }
            }

            return _currentTargetId;
        }

        private bool IsTargetValid(int targetId)
        {
            return targetId >= 0 && projectileSystem.TargetProvider.TryGetTarget(targetId, out var target) && target != null && target.IsAlive;
        }

        private bool HasLineOfSight(Vector3 origin, int targetId)
        {
            if (!projectileSystem.TargetProvider.TryGetTarget(targetId, out var target) || target == null)
            {
                return false;
            }

            var targetPosition = target.Position;
            var direction = targetPosition - origin;
            var distance = direction.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return true;
            }

            if (!Physics.Raycast(origin, direction.normalized, out var hit, distance, ~0, QueryTriggerInteraction.Collide))
            {
                return true;
            }

            return hit.collider != null && hit.collider.TryGetComponent<WaveGame.Combat.Damage.IDamageable>(out var damageable) && damageable.EntityId == targetId;
        }

        private Vector3 ResolveAimDirection(Vector3 muzzlePosition, int targetId)
        {
            if (targetId >= 0 && projectileSystem.TargetProvider.TryGetTarget(targetId, out var target) && target != null)
            {
                var toTarget = target.Position - muzzlePosition;
                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    return toTarget.normalized;
                }
            }

            return transform.forward;
        }

        private static Vector3 GetRandomPlanarDirection()
        {
            var random = Random.insideUnitCircle.normalized;
            if (random.sqrMagnitude <= Mathf.Epsilon)
            {
                return Vector3.forward;
            }

            return new Vector3(random.x, 0f, random.y);
        }

        private static Vector3 ApplySpread(Vector3 baseDirection, float spreadAngle, int index, int total)
        {
            if (total <= 1 || spreadAngle <= 0f)
            {
                return baseDirection;
            }

            var t = total == 1 ? 0.5f : index / (float)(total - 1);
            var angle = Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, t);
            return Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;
        }
    }
}
