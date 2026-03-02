using UnityEngine;
using WaveGame.Combat.Damage;

namespace WaveGame.Combat.Projectiles
{
    public interface IProjectileArchetype
    {
        void Simulate(ref ProjectileInstance projectile, float dt, IProjectileContext context);
    }

    public interface IProjectileTargetProvider
    {
        bool TryGetTarget(int entityId, out IDamageable target);
        int AcquireTarget(Vector3 origin, Vector3 forward, float radius, float preferForwardAngleDeg, int teamId);
    }

    public interface IProjectileContext
    {
        float TimeNow { get; }
        HitRegistry HitRegistry { get; }
        IProjectileTargetProvider TargetProvider { get; }
        int SphereCastNonAlloc(Vector3 origin, float radius, Vector3 direction, RaycastHit[] hits, float maxDistance, LayerMask layerMask);
        int OverlapSphereNonAlloc(Vector3 center, float radius, Collider[] colliders, LayerMask layerMask);
        void EnqueueHit(in HitEvent hitEvent);
    }
}
