using UnityEngine;

namespace WaveGame.Combat.Enemy
{
    [CreateAssetMenu(menuName = "WaveGame/Combat/Boss Loot", fileName = "BossLoot")]
    public sealed class BossLootSO : ScriptableObject
    {
        [Min(0f)] public float BonusXp = 20f;
        [Range(0f, 1f)] public float RareRewardChance = 0.2f;
    }
}
