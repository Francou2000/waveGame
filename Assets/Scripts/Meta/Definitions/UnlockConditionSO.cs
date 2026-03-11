using UnityEngine;

namespace WaveGame.Meta.Definitions
{
    public enum UnlockConditionType
    {
        ReachLevel,
        SurviveMinutes,
        DefeatBoss,
        KillEnemyTypeCount,
        CompleteRunOnDifficulty
    }

    [CreateAssetMenu(menuName = "WaveGame/Meta/Unlock Condition", fileName = "UnlockCondition")]
    public sealed class UnlockConditionSO : ScriptableObject
    {
        public UnlockConditionType ConditionType;
        [Min(0)] public int RequiredInt;
        [Min(0f)] public float RequiredFloat;
        public string RequiredId;
    }
}
