using UnityEngine;

namespace WaveGame.Meta.Definitions
{
    public enum EvolutionTriggerSource
    {
        ChestOnly,
        LevelUpAllowed,
        BossReward,
        Any
    }

    [CreateAssetMenu(menuName = "WaveGame/Meta/Evolution Recipe", fileName = "EvolutionRecipe")]
    public sealed class EvolutionRecipeSO : ScriptableObject
    {
        public string BaseWeaponId;
        public string RequiredPassiveId;
        [Min(1)] public int MinBaseWeaponLevel = 8;
        public EvolutionTriggerSource TriggerSource = EvolutionTriggerSource.ChestOnly;
        public string ResultWeaponId;
        [Range(0f, 10f)] public float Weight = 1f;
    }
}
