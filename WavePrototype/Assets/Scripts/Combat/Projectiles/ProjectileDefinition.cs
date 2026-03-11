using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    [CreateAssetMenu(menuName = "WaveGame/Combat/Projectile Definition", fileName = "ProjectileDefinition")]
    public sealed class ProjectileDefinition : ScriptableObject
    {
        [Header("General")]
        public ProjectileArchetypeType ArchetypeType = ProjectileArchetypeType.Straight;
        public float LifetimeSeconds = 5f;
        public float MaxDistance = 100f;

        [Header("Movement")]
        public float Speed = 25f;
        public float Acceleration;
        public float TurnRate = 360f;
        public float GravityScale;
        [Range(0f, 1f)] public float Drag;

        [Header("Collision")]
        public float Radius = 0.15f;
        public LayerMask HitMask;
        public bool StopOnWorld = true;
        public bool StopOnEnemy = true;

        [Header("Impact Rules")]
        public int PierceCount;
        public int BounceCount;
        public int MaxHitsTotal = 1;
        [Range(0f, 1f)] public float FalloffPerPierce;
        [Range(0f, 1f)] public float FalloffPerBounce;

        [Header("Damage")]
        public float BaseDamage = 10f;
        public DamageType DamageType = DamageType.Physical;
        [Range(0f, 1f)] public float CritChance;
        public float CritMultiplier = 2f;
        public float KnockbackForce;

        [Header("Tick & Targeting")]
        public float TickInterval = 0.1f;
        public float HitCooldownPerTarget = 0.1f;
        public float AcquireRadius = 20f;
        public float RetargetInterval = 0.35f;
        [Range(0f, 180f)] public float PreferForwardAngle = 45f;

        [Header("Presentation")]
        public string VisualPrefabId;
        public string ImpactVfxId;
        public string LoopVfxId;
        public string SfxId;
    }
}
