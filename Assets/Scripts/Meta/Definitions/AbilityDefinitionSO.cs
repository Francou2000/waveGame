using UnityEngine;

namespace WaveGame.Meta.Definitions
{
    [CreateAssetMenu(menuName = "WaveGame/Meta/Ability Definition", fileName = "AbilityDefinition")]
    public sealed class AbilityDefinitionSO : ScriptableObject, IContentDefinition
    {
        [SerializeField] private string contentId = "ability.base";
        [SerializeField] private ContentRarity rarity = ContentRarity.Common;
        [TextArea] public string Description;

        public string ContentId => contentId;
        public ContentRarity Rarity => rarity;
    }
}
