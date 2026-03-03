using UnityEngine;

namespace WaveGame.Combat.Player
{
    [CreateAssetMenu(menuName = "WaveGame/Combat/Player Stats Definition", fileName = "PlayerStatsDefinition")]
    public sealed class PlayerStatsDefinitionSO : ScriptableObject
    {
        [Min(1f)] public float MaxHealth = 100f;
        [Min(0.1f)] public float MoveSpeed = 6f;
        [Min(0.1f)] public float AttackSpeedMultiplier = 1f;
    }
}
