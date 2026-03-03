using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    public enum FirePatternType
    {
        Single,
        SpreadCone,
        Burst,
        Spiral,
        Alternating,
        RandomCone
    }

    [CreateAssetMenu(menuName = "WaveGame/Combat/Fire Pattern", fileName = "FirePattern")]
    public sealed class FirePatternSO : ScriptableObject
    {
        public FirePatternType PatternType = FirePatternType.Single;

        [Header("Spread / Cone")]
        [Range(0f, 180f)] public float ConeAngleDeg = 0f;

        [Header("Burst")]
        [Min(1)] public int BurstCount = 1;
        [Min(0f)] public float BurstInterval = 0.05f;

        [Header("Spiral")]
        public float AngularSpeedDegPerSec = 120f;

        [Header("Alternating")]
        public float LateralOffset = 0.25f;
    }
}
