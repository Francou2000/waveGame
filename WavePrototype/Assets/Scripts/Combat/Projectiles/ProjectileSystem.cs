using System.Collections.Generic;
using UnityEngine;
using WaveGame.Combat.Damage;
using WaveGame.Combat.Projectiles.Archetypes;

namespace WaveGame.Combat.Projectiles
{
    public sealed class ProjectileSystem : MonoBehaviour, IProjectileContext
    {
        [SerializeField] private ProjectileGlobalConfig globalConfig;
        [SerializeField] private LayerMask enemyMask;

        [Header("Debug Visuals")]
        [SerializeField] private GameObject projectileVisualPrefab;

        [Header("Gameplay Feedback")]
        [SerializeField] private GameObject impactVfxPrefab;
        [SerializeField, Min(0.05f)] private float impactVfxLifetime = 0.5f;
        [SerializeField] private GameObject damagePopupPrefab;
        [SerializeField, Min(0.05f)] private float damagePopupLifetime = 0.6f;

        private struct ActiveFx
        {
            public GameObject Instance;
            public float DespawnTime;
        }

        private readonly List<ProjectileInstance> _active = new(1024);
        private readonly Queue<HitEvent> _hitQueue = new(512);
        private readonly Dictionary<ProjectileArchetypeType, IProjectileArchetype> _archetypes = new();
        private readonly Dictionary<int, Transform> _visualByProjectileId = new();
        private readonly List<ActiveFx> _activeImpactVfx = new(256);
        private readonly List<ActiveFx> _activeDamagePopups = new(128);

        private ProjectilePhysicsTargetProvider _targetProvider;
        private HitResolver _hitResolver;
        private int _nextProjectileId;
        private int _physicsQueries;
        private int _peakActiveProjectiles;

        public float TimeNow => Time.time;
        public HitRegistry HitRegistry { get; } = new();
        public IProjectileTargetProvider TargetProvider => _targetProvider;
        public int ActiveProjectileCount => _active.Count;
        public int PhysicsQueriesThisFrame => _physicsQueries;
        public int PeakActiveProjectiles => _peakActiveProjectiles;

        private void Awake()
        {
            var bufferSize = globalConfig != null ? globalConfig.NonAllocHitBufferSize : 128;

            _targetProvider = new ProjectilePhysicsTargetProvider(bufferSize, enemyMask);
            _hitResolver = new HitResolver(_targetProvider);

            _archetypes[ProjectileArchetypeType.Straight] = new StraightProjectileArchetype(bufferSize);
            _archetypes[ProjectileArchetypeType.Homing] = new HomingProjectileArchetype(bufferSize);
            _archetypes[ProjectileArchetypeType.Hitscan] = new HitscanProjectileArchetype(bufferSize);
            _archetypes[ProjectileArchetypeType.AoE] = new AoEProjectileArchetype(bufferSize);
            _archetypes[ProjectileArchetypeType.Beam] = new BeamProjectileArchetype(bufferSize);
            _archetypes[ProjectileArchetypeType.Aura] = new AuraProjectileArchetype(bufferSize);
        }

        private void Update()
        {
            _physicsQueries = 0;
            SimulateProjectiles(Time.deltaTime);
            ResolveHits();
            UpdateFeedbackFx();
        }

        public bool TrySpawn(in ProjectileSpawnContext spawn)
        {
            if (spawn.Definition == null)
            {
                return false;
            }

            var maxActive = globalConfig != null ? globalConfig.MaxActiveProjectiles : 1024;
            if (_active.Count >= maxActive)
            {
                RecycleAt(0, _active[0].InstanceId);
            }

            var instance = ProjectileInstance.Create(++_nextProjectileId, spawn.Definition, spawn.OwnerEntityId, spawn.TeamId, spawn.Position, spawn.Direction);

            if (spawn.TargetEntityId >= 0)
            {
                instance.TargetEntityId = spawn.TargetEntityId;
            }
            else if (spawn.Definition.ArchetypeType == ProjectileArchetypeType.Homing)
            {
                instance.TargetEntityId = _targetProvider.AcquireTarget(spawn.Position, spawn.Direction, spawn.Definition.AcquireRadius, spawn.Definition.PreferForwardAngle, spawn.TeamId);
            }

            if (spawn.Definition.ArchetypeType == ProjectileArchetypeType.Homing)
            {
                instance.NextRetargetTime = TimeNow + spawn.Definition.RetargetInterval;
            }

            _active.Add(instance);
            if (_active.Count > _peakActiveProjectiles)
            {
                _peakActiveProjectiles = _active.Count;
            }

            CreateVisualFor(instance);
            return true;
        }

        private void SimulateProjectiles(float dt)
        {
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var projectile = _active[i];
                projectile.TimeAlive += dt;

                if (projectile.TimeAlive > projectile.Definition.LifetimeSeconds || projectile.Travelled > projectile.Definition.MaxDistance)
                {
                    RecycleAt(i, projectile.InstanceId);
                    continue;
                }

                if (!_archetypes.TryGetValue(projectile.Definition.ArchetypeType, out var archetype))
                {
                    RecycleAt(i, projectile.InstanceId);
                    continue;
                }

                archetype.Simulate(ref projectile, dt, this);
                _active[i] = projectile;
                SyncVisual(projectile);

                if (projectile.IsFinished)
                {
                    RecycleAt(i, projectile.InstanceId);
                }
            }
        }

        private void ResolveHits()
        {
            while (_hitQueue.Count > 0)
            {
                var hitEvent = _hitQueue.Dequeue();
                SpawnImpactFx(hitEvent.HitPoint, hitEvent.HitNormal);

                if (_hitResolver.Resolve(hitEvent, out var resolvedDamage, out var isCritical))
                {
                    SpawnDamagePopup(hitEvent.HitPoint, resolvedDamage, isCritical);
                }
            }
        }

        private void RecycleAt(int index, int projectileId)
        {
            HitRegistry.ForgetProjectile(projectileId);
            DestroyVisual(projectileId);
            _active.RemoveAt(index);
        }

        private void CreateVisualFor(in ProjectileInstance instance)
        {
            if (projectileVisualPrefab == null)
            {
                return;
            }

            var direction = instance.Direction.sqrMagnitude > 0.0001f ? instance.Direction : Vector3.forward;
            var rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            var visual = Instantiate(projectileVisualPrefab, instance.Position, rotation, transform);
            _visualByProjectileId[instance.InstanceId] = visual.transform;
        }

        private void SyncVisual(in ProjectileInstance instance)
        {
            if (!_visualByProjectileId.TryGetValue(instance.InstanceId, out var visual) || visual == null)
            {
                return;
            }

            visual.position = instance.Position;
            if (instance.Direction.sqrMagnitude > 0.0001f)
            {
                visual.rotation = Quaternion.LookRotation(instance.Direction.normalized, Vector3.up);
            }
        }

        private void DestroyVisual(int projectileId)
        {
            if (!_visualByProjectileId.TryGetValue(projectileId, out var visual))
            {
                return;
            }

            _visualByProjectileId.Remove(projectileId);
            if (visual != null)
            {
                Destroy(visual.gameObject);
            }
        }

        private void SpawnImpactFx(Vector3 position, Vector3 normal)
        {
            if (impactVfxPrefab == null)
            {
                return;
            }

            var maxImpact = globalConfig != null ? globalConfig.MaxActiveImpactVfx : 256;
            EnsureFxCapacity(_activeImpactVfx, Mathf.Max(1, maxImpact));

            var up = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
            var rot = Quaternion.LookRotation(up);
            var go = Instantiate(impactVfxPrefab, position, rot, transform);
            _activeImpactVfx.Add(new ActiveFx { Instance = go, DespawnTime = Time.time + impactVfxLifetime });
        }

        private void SpawnDamagePopup(Vector3 position, float damage, bool isCritical)
        {
            if (damagePopupPrefab == null)
            {
                return;
            }

            var maxPopups = globalConfig != null ? globalConfig.MaxActiveDamagePopups : 128;
            EnsureFxCapacity(_activeDamagePopups, Mathf.Max(1, maxPopups));

            var popupPos = position + Vector3.up * 0.25f;
            var go = Instantiate(damagePopupPrefab, popupPos, Quaternion.identity, transform);
            var scale = isCritical ? 1.35f : 1f;
            go.transform.localScale *= scale;
            _activeDamagePopups.Add(new ActiveFx { Instance = go, DespawnTime = Time.time + damagePopupLifetime });
        }

        private static void EnsureFxCapacity(List<ActiveFx> list, int max)
        {
            if (list.Count < max)
            {
                return;
            }

            var first = list[0];
            if (first.Instance != null)
            {
                Object.Destroy(first.Instance);
            }

            list.RemoveAt(0);
        }

        private void UpdateFeedbackFx()
        {
            CleanupFxList(_activeImpactVfx);
            CleanupFxList(_activeDamagePopups);
        }

        private static void CleanupFxList(List<ActiveFx> list)
        {
            var now = Time.time;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var fx = list[i];
                if (fx.Instance == null || now >= fx.DespawnTime)
                {
                    if (fx.Instance != null)
                    {
                        Object.Destroy(fx.Instance);
                    }

                    list.RemoveAt(i);
                }
            }
        }

        public int SphereCastNonAlloc(Vector3 origin, float radius, Vector3 direction, RaycastHit[] hits, float maxDistance, LayerMask layerMask)
        {
            if (!HasQueryBudget())
            {
                return 0;
            }

            _physicsQueries++;
            return Physics.SphereCastNonAlloc(origin, radius, direction, hits, maxDistance, layerMask, QueryTriggerInteraction.Collide);
        }

        public int OverlapSphereNonAlloc(Vector3 center, float radius, Collider[] colliders, LayerMask layerMask)
        {
            if (!HasQueryBudget())
            {
                return 0;
            }

            _physicsQueries++;
            return Physics.OverlapSphereNonAlloc(center, radius, colliders, layerMask, QueryTriggerInteraction.Collide);
        }

        private bool HasQueryBudget()
        {
            if (globalConfig == null)
            {
                return true;
            }

            return _physicsQueries < globalConfig.MaxPhysicsQueriesPerFrame;
        }

        private void OnDisable()
        {
            foreach (var pair in _visualByProjectileId)
            {
                if (pair.Value != null)
                {
                    Destroy(pair.Value.gameObject);
                }
            }

            _visualByProjectileId.Clear();
            ForceCleanupFx(_activeImpactVfx);
            ForceCleanupFx(_activeDamagePopups);
        }

        private static void ForceCleanupFx(List<ActiveFx> list)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].Instance != null)
                {
                    Object.Destroy(list[i].Instance);
                }
            }

            list.Clear();
        }

        public void RegisterTarget(IDamageable target)
        {
            _targetProvider.Register(target);
        }

        public void EnqueueHit(in HitEvent hitEvent)
        {
            _hitQueue.Enqueue(hitEvent);
        }
    }
}
