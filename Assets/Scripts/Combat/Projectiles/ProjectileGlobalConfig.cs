using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    [CreateAssetMenu(menuName = "WaveGame/Combat/Projectile Global Config", fileName = "ProjectileGlobalConfig")]
    public sealed class ProjectileGlobalConfig : ScriptableObject
    {
        [Min(1)] public int MaxActiveProjectiles = 1024;
        [Min(1)] public int MaxActiveBeams = 16;
        [Min(1)] public int MaxActiveImpactVfx = 256;
        [Min(1)] public int MaxActiveDamagePopups = 128;
        [Min(64)] public int MaxPhysicsQueriesPerFrame = 4096;
        [Min(1)] public int NonAllocHitBufferSize = 128;
    }
}
