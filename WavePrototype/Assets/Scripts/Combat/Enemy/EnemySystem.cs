using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace WaveGame.Combat.Enemy
{
    public sealed class EnemySystem : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform playerTarget;

        [Header("Query")]
        [SerializeField] private LayerMask enemyLayerMask;
        [SerializeField, Min(4)] private int maxNeighborsPerEnemy = 16;

        [Header("Scheduling")]
        [SerializeField, Min(1)] private int steeringBuckets = 12;
        [SerializeField, Min(0.01f)] private float defaultSteeringInterval = 0.08f;
        [SerializeField] private bool useParallelSteering = true;
        [SerializeField, Min(8)] private int parallelSteeringThreshold = 48;

        [Header("Default Movement")]
        [SerializeField, Min(0.1f)] private float defaultMaxSpeed = 2.5f;
        [SerializeField, Min(0.1f)] private float defaultAcceleration = 12f;
        [SerializeField, Min(1f)] private float defaultTurnResponsiveness = 10f;

        [Header("Default Steering")]
        [SerializeField, Min(0f)] private float defaultSeekWeight = 1f;
        [SerializeField, Min(0f)] private float defaultSeparationWeight = 1.6f;
        [SerializeField, Min(0f)] private float defaultCohesionWeight = 0f;
        [SerializeField, Min(0.2f)] private float defaultSeparationRadius = 1f;
        [SerializeField, Min(0f)] private float defaultArriveRadius = 1.25f;
        [SerializeField, Min(0f)] private float defaultTargetOffsetRadius = 0.65f;
        [SerializeField, Min(0f)] private float defaultRandomWander = 0.12f;

        [Header("Debug")]
        [SerializeField] private bool drawDebugVectors;
        [SerializeField] private bool drawSeparationRadius;

        private struct BrainState
        {
            public Vector3 DesiredDirection;
            public Vector3 CurrentVelocity;
            public float NextSteeringTime;
            public int Bucket;
            public float RandomPhase;
            public int LastNeighborCount;
        }

        private readonly List<EnemyRuntime> _activeEnemies = new(1024);
        private readonly Dictionary<int, BrainState> _brainByEnemyId = new(1024);
        private readonly List<EnemyMovementProfileSO> _profileSnapshot = new(1024);
        private readonly List<BrainState> _stateSnapshot = new(1024);
        private readonly List<Vector3> _positionSnapshot = new(1024);
        private readonly List<Vector3> _desiredDirectionSnapshot = new(1024);
        private int _bucketCursor;

        public void Register(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return;
            }

            if (!_activeEnemies.Contains(enemy))
            {
                _activeEnemies.Add(enemy);
            }

            var enemyId = enemy.EntityId;
            var bucketCount = Mathf.Max(1, steeringBuckets);
            var bucket = Mathf.Abs(enemyId) % bucketCount;
            var phaseSeed = Mathf.Abs(enemyId) * 0.6180339f;

            _brainByEnemyId[enemyId] = new BrainState
            {
                DesiredDirection = Vector3.zero,
                CurrentVelocity = Vector3.zero,
                NextSteeringTime = Time.time + (bucket / (float)bucketCount) * Mathf.Max(0.01f, defaultSteeringInterval),
                Bucket = bucket,
                RandomPhase = phaseSeed,
                LastNeighborCount = 0
            };
        }

        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
        }

        public void Unregister(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return;
            }

            _activeEnemies.Remove(enemy);
            _brainByEnemyId.Remove(enemy.EntityId);
        }

        private void Update()
        {
            if (playerTarget == null)
            {
                return;
            }

            var now = Time.time;
            var dt = Time.deltaTime;
            var bucketCount = Mathf.Max(1, steeringBuckets);
            _bucketCursor = (_bucketCursor + 1) % bucketCount;

            for (var i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = _activeEnemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    if (enemy != null)
                    {
                        _brainByEnemyId.Remove(enemy.EntityId);
                    }

                    _activeEnemies.RemoveAt(i);
                    continue;
                }

                var enemyId = enemy.EntityId;
                if (!_brainByEnemyId.TryGetValue(enemyId, out var state))
                {
                    var bucket = Mathf.Abs(enemyId) % bucketCount;
                    state = new BrainState
                    {
                        DesiredDirection = Vector3.zero,
                        CurrentVelocity = Vector3.zero,
                        NextSteeringTime = now,
                        Bucket = bucket,
                        RandomPhase = Mathf.Abs(enemyId) * 0.6180339f,
                        LastNeighborCount = 0
                    };
                }
            }

            BuildSnapshots(now);
            ComputeDesiredDirections(now);

            for (var i = 0; i < _activeEnemies.Count; i++)
            {
                var enemy = _activeEnemies[i];
                var enemyId = enemy.EntityId;
                var state = _stateSnapshot[i];
                var profile = _profileSnapshot[i];

                var steeringInterval = Mathf.Max(0.01f, profile != null ? profile.UpdateInterval : defaultSteeringInterval);
                var shouldSteer = state.Bucket == _bucketCursor && now >= state.NextSteeringTime;
                if (shouldSteer)
                {
                    state.DesiredDirection = _desiredDirectionSnapshot[i];
                    state.NextSteeringTime = now + steeringInterval;
                }

                ApplyMovement(enemy, profile, dt, ref state);
                _brainByEnemyId[enemyId] = state;
            }
        }

        private void BuildSnapshots(float now)
        {
            _profileSnapshot.Clear();
            _stateSnapshot.Clear();
            _positionSnapshot.Clear();
            _desiredDirectionSnapshot.Clear();

            for (var i = 0; i < _activeEnemies.Count; i++)
            {
                var enemy = _activeEnemies[i];
                var enemyId = enemy.EntityId;
                var profile = enemy.Definition != null ? enemy.Definition.MovementProfile : null;

                if (!_brainByEnemyId.TryGetValue(enemyId, out var state))
                {
                    var bucketCount = Mathf.Max(1, steeringBuckets);
                    var bucket = Mathf.Abs(enemyId) % bucketCount;
                    state = new BrainState
                    {
                        DesiredDirection = Vector3.zero,
                        CurrentVelocity = Vector3.zero,
                        NextSteeringTime = now,
                        Bucket = bucket,
                        RandomPhase = Mathf.Abs(enemyId) * 0.6180339f,
                        LastNeighborCount = 0
                    };
                }

                _profileSnapshot.Add(profile);
                _stateSnapshot.Add(state);
                _positionSnapshot.Add(enemy.transform.position);
                _desiredDirectionSnapshot.Add(state.DesiredDirection);
            }
        }

        private void ComputeDesiredDirections(float now)
        {
            var count = _activeEnemies.Count;
            if (count == 0)
            {
                return;
            }

            void ComputeAt(int i)
            {
                var enemy = _activeEnemies[i];
                var state = _stateSnapshot[i];
                var profile = _profileSnapshot[i];
                var shouldSteer = state.Bucket == _bucketCursor && now >= state.NextSteeringTime;
                if (!shouldSteer)
                {
                    _desiredDirectionSnapshot[i] = state.DesiredDirection;
                    return;
                }

                var seekWeight = profile != null ? profile.SeekWeight : defaultSeekWeight;
                var separationWeight = profile != null ? profile.SeparationWeight : defaultSeparationWeight;
                var cohesionWeight = profile != null ? profile.CohesionWeight : defaultCohesionWeight;
                var randomWander = profile != null ? profile.RandomWander : defaultRandomWander;

                var seek = ComputeSeek(i, enemy, profile);
                var separation = ComputeSeparationFromSnapshot(i, profile, out var neighborCount);
                var cohesion = cohesionWeight > 0f ? ComputeCohesionFromSnapshot(i, profile, neighborCount) : Vector3.zero;
                var wander = ComputeWander(now, state.RandomPhase, randomWander);
                state.LastNeighborCount = neighborCount;

                var desired = (seek * seekWeight) + (separation * separationWeight) + (cohesion * cohesionWeight) + wander;
                desired.y = 0f;
                _desiredDirectionSnapshot[i] = desired.sqrMagnitude > 0.0001f ? desired.normalized : Vector3.zero;
                _stateSnapshot[i] = state;
            }

            if (useParallelSteering && count >= parallelSteeringThreshold && System.Environment.ProcessorCount > 1)
            {
                Parallel.For(0, count, ComputeAt);
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    ComputeAt(i);
                }
            }
        }

        private Vector3 ComputeSeek(int index, EnemyRuntime enemy, EnemyMovementProfileSO profile)
        {
            var enemyPos = _positionSnapshot[index];
            var targetPos = playerTarget.position + GetTargetOffset(enemy, profile);
            var toTarget = targetPos - enemyPos;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            var arriveRadius = Mathf.Max(0f, profile != null ? profile.ArriveRadius : defaultArriveRadius);
            var distance = toTarget.magnitude;
            var seek = toTarget / distance;

            if (arriveRadius > 0f && distance < arriveRadius)
            {
                seek *= distance / arriveRadius;
            }

            return seek;
        }

        private Vector3 GetTargetOffset(EnemyRuntime enemy, EnemyMovementProfileSO profile)
        {
            var radius = Mathf.Max(0f, profile != null ? profile.TargetOffsetRadius : defaultTargetOffsetRadius);
            if (radius <= 0f)
            {
                return Vector3.zero;
            }

            var angle = Mathf.Abs(enemy.EntityId) * 0.75487766f;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        }

        private Vector3 ComputeSeparationFromSnapshot(int index, EnemyMovementProfileSO profile, out int processedNeighbors)
        {
            var radius = Mathf.Max(0.2f, profile != null ? profile.SeparationRadius : defaultSeparationRadius);
            var center = _positionSnapshot[index];
            var push = Vector3.zero;
            processedNeighbors = 0;
            var cap = Mathf.Max(1, maxNeighborsPerEnemy);
            var radiusSqr = radius * radius;

            for (var i = 0; i < _positionSnapshot.Count && processedNeighbors < cap; i++)
            {
                if (i == index)
                {
                    continue;
                }

                var away = center - _positionSnapshot[i];
                away.y = 0f;
                var distanceSqr = away.sqrMagnitude;
                if (distanceSqr <= 0.0001f || distanceSqr > radiusSqr)
                {
                    continue;
                }

                var distance = Mathf.Sqrt(distanceSqr);
                var strength = 1f - (distance / radius);
                push += away / distance * strength;
                processedNeighbors++;
            }

            if (processedNeighbors > 0)
            {
                push /= processedNeighbors;
            }

            if (push.sqrMagnitude > 0.0001f)
            {
                push.Normalize();
            }

            return push;
        }

        private Vector3 ComputeCohesionFromSnapshot(int index, EnemyMovementProfileSO profile, int neighborCount)
        {
            if (neighborCount <= 0)
            {
                return Vector3.zero;
            }

            var radius = Mathf.Max(0.2f, profile != null ? profile.SeparationRadius : defaultSeparationRadius) * 1.5f;
            var center = _positionSnapshot[index];
            var average = Vector3.zero;
            var count = 0;
            var cap = Mathf.Max(1, maxNeighborsPerEnemy);
            var radiusSqr = radius * radius;

            for (var i = 0; i < _positionSnapshot.Count && count < cap; i++)
            {
                if (i == index)
                {
                    continue;
                }

                var toOther = _positionSnapshot[i] - center;
                toOther.y = 0f;
                if (toOther.sqrMagnitude > radiusSqr)
                {
                    continue;
                }

                average += _positionSnapshot[i];
                count++;
            }

            if (count == 0)
            {
                return Vector3.zero;
            }

            average /= count;
            var toCenter = average - center;
            toCenter.y = 0f;

            if (toCenter.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            return toCenter.normalized;
        }

        private static Vector3 ComputeWander(float now, float phase, float amplitude)
        {
            if (amplitude <= 0f)
            {
                return Vector3.zero;
            }

            var x = Mathf.Sin((now * 1.7f) + phase);
            var z = Mathf.Cos((now * 1.31f) + phase * 1.37f);
            return new Vector3(x, 0f, z).normalized * amplitude;
        }

        private void ApplyMovement(EnemyRuntime enemy, EnemyMovementProfileSO profile, float dt, ref BrainState state)
        {
            var maxSpeed = Mathf.Max(0.1f, profile != null ? profile.MaxSpeed : (enemy.MoveSpeed > 0f ? enemy.MoveSpeed : defaultMaxSpeed));
            var acceleration = Mathf.Max(0.1f, profile != null ? profile.Acceleration : defaultAcceleration);
            var turn = Mathf.Max(1f, profile != null ? profile.TurnResponsiveness : defaultTurnResponsiveness);

            var targetVelocity = state.DesiredDirection * maxSpeed;
            state.CurrentVelocity = Vector3.MoveTowards(state.CurrentVelocity, targetVelocity, acceleration * dt);

            state.CurrentVelocity.y = 0f;
            if (state.CurrentVelocity.sqrMagnitude > 0.0001f)
            {
                enemy.transform.position += state.CurrentVelocity * dt;

                var facing = state.CurrentVelocity.normalized;
                var targetRotation = Quaternion.LookRotation(facing, Vector3.up);
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, turn * dt);
            }

            if (!drawDebugVectors)
            {
                return;
            }

            var origin = enemy.transform.position + Vector3.up * 0.1f;
            Debug.DrawRay(origin, state.DesiredDirection * 1.2f, Color.yellow);
            Debug.DrawRay(origin, state.CurrentVelocity, Color.cyan);

            if (drawSeparationRadius)
            {
                var radius = Mathf.Max(0.2f, profile != null ? profile.SeparationRadius : defaultSeparationRadius);
                DrawCircleXZ(origin, radius, Color.magenta);
            }
        }

        private static void DrawCircleXZ(Vector3 center, float radius, Color color)
        {
            const int segments = 20;
            var prev = center + new Vector3(radius, 0f, 0f);

            for (var i = 1; i <= segments; i++)
            {
                var t = i / (float)segments;
                var angle = t * Mathf.PI * 2f;
                var next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Debug.DrawLine(prev, next, color);
                prev = next;
            }
        }
    }
}
