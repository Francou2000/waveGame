using UnityEngine;

namespace WaveGame.Meta.Definitions
{
    [CreateAssetMenu(menuName = "WaveGame/Meta/Passive Definition", fileName = "PassiveDefinition")]
    public sealed class PassiveDefinitionSO : ScriptableObject, IContentDefinition
    {
        [SerializeField] private string contentId = "passive.base";
        [SerializeField] private ContentRarity rarity = ContentRarity.Common;
        [TextArea] public string Description;

        public string ContentId => contentId;
        public ContentRarity Rarity => rarity;
    }
}
