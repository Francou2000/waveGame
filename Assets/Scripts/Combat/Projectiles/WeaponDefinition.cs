using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    public enum TargetingMode
    {
        Nearest,
        ForwardCone,
        RandomInRange
    }

    [CreateAssetMenu(menuName = "WaveGame/Combat/Weapon Definition", fileName = "WeaponDefinition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("Cadence")]
        [Min(0.01f)] public float BaseCooldown = 0.8f;
        [Min(1)] public int ProjectilesPerShot = 1;
        [Min(1)] public int BurstCount = 1;
        [Min(0f)] public float BurstInterval = 0.05f;

        [Header("Pattern")]
        [Range(0f, 180f)] public float SpreadAngle = 0f;

        [Header("Targeting")]
        [Min(0.1f)] public float Range = 18f;
        public TargetingMode TargetingMode = TargetingMode.Nearest;
        [Range(0f, 180f)] public float ConeAngle = 70f;
        public bool RequiresLineOfSight;
        [Min(0.05f)] public float RetargetInterval = 0.2f;

        [Header("Projectile")]
        public ProjectileDefinition ProjectileDefinition;

        [Header("Spawn")]
        public Vector3 MuzzleOffset = new(0f, 1f, 0.8f);
    }
}
