using UnityEngine;

namespace WaveGame.Combat.Enemy
{
    [CreateAssetMenu(menuName = "WaveGame/Combat/Boss Phase Profile", fileName = "BossPhaseProfile")]
    public sealed class BossPhaseProfileSO : ScriptableObject
    {
        [Range(0f, 1f)] public float[] PhaseHpThresholds = { 0.66f, 0.33f };
        public float[] MoveSpeedMultipliers = { 1f, 1.2f, 1.4f };
    }
}
