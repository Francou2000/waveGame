using UnityEngine;
using UnityEngine.InputSystem;
using WaveGame.Combat.Player;

namespace WaveGame.Combat.Projectiles
{
    public sealed class AutoFireWeaponEmitter : MonoBehaviour
    {
        [SerializeField] private ProjectileSystem projectileSystem;
        [SerializeField] private WeaponDefinition weaponDefinition;
        [SerializeField] private int ownerEntityId = 1;
        [SerializeField] private int teamId = 1;
        [SerializeField] private PlayerCombatAnchorProvider combatSource;
        [SerializeField] private PlayerStatsRuntime playerStats;

        [Header("Runtime stats")]
        [SerializeField] private float attackSpeedMultiplier = 1f;
        [SerializeField] private int additionalProjectiles;

        [Header("Testing")]
        [SerializeField] private bool autoFireEnabled = true;
        [SerializeField] private bool requireAttackInput;

        [Header("Input")]
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string attackActionName = "Attack";

        private int _cachedTargetId = -1;
        private float _nextRetargetTime;
        private float _nextFireTime;
        private int _pendingBurstShots;
        private float _nextBurstShotTime;
        private float _patternAngle;
        private bool _alternatingSide;
        private InputAction _resolvedAttackAction;

        private void Awake()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }

            if (combatSource == null)
            {
                combatSource = GetComponent<PlayerCombatAnchorProvider>();
            }

            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStatsRuntime>();
            }

            ResolveAttackAction();
        }

        private void OnEnable()
        {
            ResolveAttackAction();

            if (attackAction != null)
            {
                _resolvedAttackAction?.Enable();
            }
        }

        private void OnDisable()
        {
            if (attackAction != null)
            {
                _resolvedAttackAction?.Disable();
            }
        }

        private void Update()
        {
            if (!CanRun())
            {
                _pendingBurstShots = 0;
                return;
            }

            var now = Time.time;
            if (_pendingBurstShots > 0)
            {
                if (now >= _nextBurstShotTime)
                {
                    FireVolley(now);
                    _pendingBurstShots--;
                    _nextBurstShotTime = now + GetBurstInterval();
                }

                return;
            }

            if (now < _nextFireTime)
            {
                return;
            }

            _pendingBurstShots = GetBurstCount();
            _nextFireTime = now + GetFinalCooldown();

            if (_pendingBurstShots > 0)
            {
                FireVolley(now);
                _pendingBurstShots--;
                _nextBurstShotTime = now + GetBurstInterval();
            }
        }

        private bool CanRun()
        {
            if (!autoFireEnabled || projectileSystem == null || projectileSystem.TargetProvider == null || weaponDefinition == null || weaponDefinition.ProjectileDefinition == null)
            {
                return false;
            }

            return !requireAttackInput || IsAttackPressed();
        }

        private bool IsAttackPressed()
        {
            if (_resolvedAttackAction == null)
            {
                ResolveAttackAction();
            }

            return _resolvedAttackAction != null && _resolvedAttackAction.IsPressed();
        }

        private void ResolveAttackAction()
        {
            _resolvedAttackAction = attackAction != null ? attackAction.action : null;

            if (_resolvedAttackAction == null && playerInput != null && playerInput.actions != null && !string.IsNullOrWhiteSpace(attackActionName))
            {
                _resolvedAttackAction = playerInput.actions.FindAction(attackActionName, false);
            }
        }

        private float GetFinalCooldown()
        {
            var statSpeed = playerStats != null ? playerStats.AttackSpeedMultiplier : 1f;
            var speed = Mathf.Max(0.01f, attackSpeedMultiplier * statSpeed);
            return Mathf.Max(0.08f, weaponDefinition.BaseCooldown / speed);
        }

        private int GetBurstCount()
        {
            return weaponDefinition.FirePattern != null && weaponDefinition.FirePattern.PatternType == FirePatternType.Burst
                ? Mathf.Max(1, weaponDefinition.FirePattern.BurstCount)
                : 1;
        }

        private float GetBurstInterval()
        {
            return weaponDefinition.FirePattern != null && weaponDefinition.FirePattern.PatternType == FirePatternType.Burst
                ? Mathf.Max(0f, weaponDefinition.FirePattern.BurstInterval)
                : 0f;
        }

        private void FireVolley(float now)
        {
            var muzzle = GetMuzzlePosition();
            var targetId = AcquireTarget(muzzle, now);
            var baseDir = ResolveAimDirection(muzzle, targetId);

            var count = Mathf.Max(1, weaponDefinition.ProjectilesPerShot + additionalProjectiles);
            for (var i = 0; i < count; i++)
            {
                var dir = ApplyPattern(baseDir, i, count, now);
                projectileSystem.TrySpawn(new ProjectileSpawnContext(weaponDefinition.ProjectileDefinition, ownerEntityId, teamId, muzzle, dir, targetId));
            }
        }

        private int AcquireTarget(Vector3 origin, float now)
        {
            if (now < _nextRetargetTime && IsTargetValid(_cachedTargetId))
            {
                return _cachedTargetId;
            }

            var targeting = weaponDefinition.Targeting;
            var range = targeting != null ? targeting.AcquireRadius : weaponDefinition.Range;
            var cone = 180f;
            var forward = GetForward();

            if (targeting != null)
            {
                if (targeting.Mode == TargetingModeSO.ForwardConeNearest)
                {
                    cone = targeting.ConeAngleDeg;
                }
                else if (targeting.Mode == TargetingModeSO.RandomInRange)
                {
                    forward = GetRandomPlanarDirection();
                }
            }

            _cachedTargetId = projectileSystem.TargetProvider.AcquireTarget(origin, forward, range, cone, teamId);
            _nextRetargetTime = now + (targeting != null ? Mathf.Max(0.05f, targeting.RetargetInterval) : 0.2f);

            if ((targeting != null && targeting.RequireLineOfSight) || weaponDefinition.RequiresLineOfSight)
            {
                if (!HasLineOfSight(origin, _cachedTargetId))
                {
                    _cachedTargetId = -1;
                }
            }

            return _cachedTargetId;
        }

        private bool IsTargetValid(int targetId)
        {
            return targetId >= 0 && projectileSystem.TargetProvider.TryGetTarget(targetId, out var t) && t != null && t.IsAlive;
        }

        private Vector3 ResolveAimDirection(Vector3 origin, int targetId)
        {
            if (targetId >= 0 && projectileSystem.TargetProvider.TryGetTarget(targetId, out var t) && t != null)
            {
                var to = t.GetAimPoint() - origin;
                if (to.sqrMagnitude > 0.0001f)
                {
                    return to.normalized;
                }
            }

            return GetForward();
        }

        private bool HasLineOfSight(Vector3 origin, int targetId)
        {
            if (targetId < 0 || !projectileSystem.TargetProvider.TryGetTarget(targetId, out var t) || t == null)
            {
                return false;
            }

            var to = t.GetAimPoint() - origin;
            var distance = to.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return true;
            }

            if (!Physics.Raycast(origin, to.normalized, out var hit, distance, ~0, QueryTriggerInteraction.Collide))
            {
                return true;
            }

            if (hit.collider == null)
            {
                return false;
            }

            var targetable = hit.collider.GetComponentInParent<WaveGame.Combat.Interfaces.ITargetable>();
            return targetable != null && targetable.EntityId == targetId;
        }

        private Vector3 GetMuzzlePosition()
        {
            var anchor = combatSource != null ? combatSource.CombatAnchor : transform;
            return anchor.TransformPoint(weaponDefinition.MuzzleLocalOffset);
        }

        private Vector3 GetForward()
        {
            var forward = combatSource != null ? combatSource.Forward : transform.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude <= 0.0001f)
            {
                return Vector3.forward;
            }

            return forward.normalized;
        }

        private Vector3 ApplyPattern(Vector3 baseDirection, int index, int total, float now)
        {
            var pattern = weaponDefinition.FirePattern;
            if (pattern == null)
            {
                return baseDirection;
            }

            switch (pattern.PatternType)
            {
                case FirePatternType.SpreadCone:
                    return ApplySpread(baseDirection, pattern.ConeAngleDeg, index, total);
                case FirePatternType.RandomCone:
                    var randomAngle = Random.Range(-pattern.ConeAngleDeg * 0.5f, pattern.ConeAngleDeg * 0.5f);
                    return Quaternion.AngleAxis(randomAngle, Vector3.up) * baseDirection;
                case FirePatternType.Spiral:
                    _patternAngle += pattern.AngularSpeedDegPerSec * Time.deltaTime;
                    return Quaternion.AngleAxis(_patternAngle, Vector3.up) * baseDirection;
                case FirePatternType.Alternating:
                    _alternatingSide = !_alternatingSide;
                    var alternatingAngle = Mathf.Max(0f, pattern.ConeAngleDeg > 0f ? pattern.ConeAngleDeg : pattern.LateralOffset * 20f);
                    var sideAngle = _alternatingSide ? alternatingAngle : -alternatingAngle;
                    return Quaternion.AngleAxis(sideAngle, Vector3.up) * baseDirection;
                default:
                    return baseDirection;
            }
        }

        private static Vector3 ApplySpread(Vector3 baseDirection, float spreadAngle, int index, int total)
        {
            if (total <= 1 || spreadAngle <= 0f)
            {
                return baseDirection;
            }

            var t = index / (float)(total - 1);
            var angle = Mathf.Lerp(-spreadAngle * 0.5f, spreadAngle * 0.5f, t);
            return Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;
        }

        private static Vector3 GetRandomPlanarDirection()
        {
            var angle = Random.value * 360f;
            var dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            return dir.normalized;
        }
    }
}
