using UnityEngine;

namespace WaveGame.Meta.Definitions
{
    [CreateAssetMenu(menuName = "WaveGame/Meta/Item Rarity Profile", fileName = "ItemRarityProfile")]
    public sealed class ItemRarityProfileSO : ScriptableObject
    {
        [Range(0f, 1f)] public float CommonWeight = 0.55f;
        [Range(0f, 1f)] public float UncommonWeight = 0.25f;
        [Range(0f, 1f)] public float RareWeight = 0.12f;
        [Range(0f, 1f)] public float EpicWeight = 0.06f;
        [Range(0f, 1f)] public float LegendaryWeight = 0.02f;

        public float GetWeight(ContentRarity rarity)
        {
            return rarity switch
            {
                ContentRarity.Common => CommonWeight,
                ContentRarity.Uncommon => UncommonWeight,
                ContentRarity.Rare => RareWeight,
                ContentRarity.Epic => EpicWeight,
                ContentRarity.Legendary => LegendaryWeight,
                _ => 0f
            };
        }
    }
}
