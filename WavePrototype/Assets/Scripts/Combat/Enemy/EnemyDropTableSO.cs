using UnityEngine;

namespace WaveGame.Combat.Enemy
{
    public enum XpOrbStrategy
    {
        SingleOrb,
        SplitOrbs,
        Tiered
    }

    [CreateAssetMenu(menuName = "WaveGame/Combat/Enemy Drop Table", fileName = "EnemyDropTable")]
    public sealed class EnemyDropTableSO : ScriptableObject
    {
        public XpOrbStrategy XpOrbStrategy = XpOrbStrategy.SingleOrb;
        [Min(0f)] public float XpAmountBase = 1f;
        [Range(0f, 1f)] public float DropVariance = 0.2f;
        [Range(0f, 1f)] public float RareDropChance;

        public float EvaluateXp(float defaultXp)
        {
            var baseValue = XpAmountBase > 0f ? XpAmountBase : defaultXp;
            var variance = 1f + Random.Range(-DropVariance, DropVariance);
            return Mathf.Max(0f, baseValue * variance);
        }
    }
}
