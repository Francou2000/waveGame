using UnityEngine;
using WaveGame.Combat.Projectiles;

namespace WaveGame.Combat.Damage
{
    public interface IDamageable
    {
        int EntityId { get; }
        int TeamId { get; }
        bool IsAlive { get; }
        Vector3 Position { get; }
        void ApplyDamage(float amount, DamageType damageType, int ownerId, bool isCritical, float knockbackForce, Vector3 hitPoint, Vector3 hitNormal);
    }
}
