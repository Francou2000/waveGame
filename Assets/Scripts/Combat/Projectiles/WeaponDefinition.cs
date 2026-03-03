using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    [CreateAssetMenu(menuName = "WaveGame/Combat/Weapon Definition", fileName = "WeaponDefinition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("Cadence")]
        [Min(0.01f)] public float BaseCooldown = 0.8f;

        [Header("Data-driven composition")]
        public FirePatternSO FirePattern;
        public TargetingDefinitionSO Targeting;
        public ProjectileDefinition ProjectileDefinition;

        [Header("Fallback / Overrides")]
        [Min(1)] public int ProjectilesPerShot = 1;
        [Min(0.1f)] public float Range = 18f;
        public bool RequiresLineOfSight;
        public Vector3 MuzzleLocalOffset = new(0f, 1f, 0.8f);

        [Header("Presentation IDs")]
        public string FireSfxId;
        public string MuzzleVfxId;
    }
}
