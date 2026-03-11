using UnityEngine;

namespace WaveGame.Combat.Enemy
{
    public enum EnemyCategory
    {
        Minion,
        Elite,
        Boss,
        Summon,
        Hazard
    }

    [CreateAssetMenu(menuName = "WaveGame/Combat/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string EnemyId = "enemy.minion";
        public EnemyCategory Category = EnemyCategory.Minion;

        [Header("Base Stats")]
        [Min(1f)] public float MaxHp = 10f;
        [Min(0f)] public float MoveSpeed = 2.5f;
        [Min(0f)] public float ContactDamage = 1f;
        [Min(0f)] public float AttackRate = 1f;
        [Range(0f, 0.95f)] public float DamageReduction;
        [Min(0f)] public float KnockbackResistance;
        [Min(0f)] public float XpValue = 1f;
        [Min(0)] public int ScoreValue = 1;

        [Header("Hitbox / Presence")]
        [Min(0.05f)] public float ColliderRadius = 0.45f;
        [Min(0.2f)] public float ColliderHeight = 1.8f;
        public Vector3 ModelScale = Vector3.one;
        public float AimPointOffset = 1f;

        [Header("Visuals")]
        public EnemyVisualSetSO VisualSet;

        [Header("Drops")]
        public EnemyDropTableSO DropTable;
        public bool DropOnDeath = true;

        [Header("Boss Extras")]
        public BossPhaseProfileSO BossPhaseProfile;
        public BossLootSO BossLoot;
        public bool IsImmuneToKnockback;
        public bool HasShieldPhases;
    }
}
