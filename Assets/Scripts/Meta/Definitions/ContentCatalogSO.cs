using System.Collections.Generic;
using UnityEngine;
using WaveGame.Combat.Projectiles;

namespace WaveGame.Meta.Definitions
{
    [CreateAssetMenu(menuName = "WaveGame/Meta/Content Catalog", fileName = "ContentCatalog")]
    public sealed class ContentCatalogSO : ScriptableObject
    {
        public List<WeaponDefinition> Weapons = new();
        public List<PassiveDefinitionSO> Passives = new();
        public List<AbilityDefinitionSO> Abilities = new();
        public List<EvolutionRecipeSO> EvolutionRecipes = new();
        public List<UnlockConditionSO> UnlockConditions = new();
        public ItemRarityProfileSO RarityProfile;
    }
}
