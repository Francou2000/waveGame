using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    public enum TargetingModeSO
    {
        Nearest,
        LowestHP,
        MostDense,
        RandomInRange,
        ForwardConeNearest
    }

    [CreateAssetMenu(menuName = "WaveGame/Combat/Targeting Definition", fileName = "TargetingDefinition")]
    public sealed class TargetingDefinitionSO : ScriptableObject
    {
        public TargetingModeSO Mode = TargetingModeSO.Nearest;
        [Min(0.1f)] public float AcquireRadius = 15f;
        [Range(0f, 180f)] public float ConeAngleDeg = 70f;
        [Min(0.05f)] public float RetargetInterval = 0.2f;
        public bool RequireLineOfSight;
        public float AimPointHeightOffset = 1f;
    }
}
