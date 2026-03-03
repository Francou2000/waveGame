using System.Collections.Generic;
using UnityEngine;
using WaveGame.Combat.Projectiles.Archetypes;

namespace WaveGame.Combat.Projectiles
{
    public sealed class ProjectileSystem : MonoBehaviour, IProjectileContext
    {
        [SerializeField] private ProjectileGlobalConfig globalConfig;
        [SerializeField] private LayerMask enemyMask;

        private readonly List<ProjectileInstance> _active = new(1024);
        private readonly Queue<HitEvent> _hitQueue = new(512);
        private readonly Dictionary<ProjectileArchetypeType, IProjectileArchetype> _archetypes = new();

        private ProjectilePhysicsTargetProvider _targetProvider;
        private HitResolver _hitResolver;
        private int _nextProjectileId;
        private int _physicsQueries;

        public float TimeNow => Time.time;
        public HitRegistry HitRegistry { get; } = new();
        public IProjectileTargetProvider TargetProvider => _targetProvider;

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
                _active.RemoveAt(0);
            }

            var instance = ProjectileInstance.Create(++_nextProjectileId, spawn.Definition, spawn.OwnerEntityId, spawn.TeamId, spawn.Position, spawn.Direction);

            if (spawn.Definition.ArchetypeType == ProjectileArchetypeType.Homing)
            {
                instance.TargetEntityId = _targetProvider.AcquireTarget(spawn.Position, spawn.Direction, spawn.Definition.AcquireRadius, spawn.Definition.PreferForwardAngle, spawn.TeamId);
                instance.NextRetargetTime = TimeNow + spawn.Definition.RetargetInterval;
            }

            _active.Add(instance);
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
                _hitResolver.Resolve(_hitQueue.Dequeue());
            }
        }

        private void RecycleAt(int index, int projectileId)
        {
            HitRegistry.ForgetProjectile(projectileId);
            _active.RemoveAt(index);
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

        public void EnqueueHit(in HitEvent hitEvent)
        {
            _hitQueue.Enqueue(hitEvent);
        }
    }
}
