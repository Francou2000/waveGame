using UnityEngine;

namespace WaveGame.Combat.Enemy
{
    [CreateAssetMenu(menuName = "WaveGame/Combat/Enemy Movement Profile", fileName = "EnemyMovementProfile")]
    public sealed class EnemyMovementProfileSO : ScriptableObject
    {
        [Header("Kinematics")]
        [Min(0.1f)] public float MaxSpeed = 2.5f;
        [Min(0.1f)] public float Acceleration = 12f;
        [Min(1f)] public float TurnResponsiveness = 10f;

        [Header("Steering Weights")]
        [Min(0f)] public float SeekWeight = 1f;
        [Min(0f)] public float SeparationWeight = 1.6f;
        [Min(0f)] public float CohesionWeight = 0f;

        [Header("Separation")]
        [Min(0.2f)] public float SeparationRadius = 1f;
        [Min(0.01f)] public float UpdateInterval = 0.08f;

        [Header("Behavior")]
        [Min(0f)] public float ArriveRadius = 1.25f;
        [Min(0f)] public float TargetOffsetRadius = 0.65f;
        [Min(0f)] public float RandomWander = 0.12f;
    }
}
