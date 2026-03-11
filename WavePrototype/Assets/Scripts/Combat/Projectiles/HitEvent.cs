using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    public struct HitEvent
    {
        public int ProjectileId;
        public int OwnerId;
        public int TargetId;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public float BaseDamage;
        public float DamageScale;
        public DamageType DamageType;
        public bool CritCandidate;
        public float CritChance;
        public float CritMultiplier;
        public float KnockbackForce;
    }
}
