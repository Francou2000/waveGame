using UnityEngine;

namespace WaveGame.Combat.Projectiles.Archetypes
{
    public sealed class HomingProjectileArchetype : IProjectileArchetype
    {
        private readonly StraightProjectileArchetype _straightDelegate;

        public HomingProjectileArchetype(int hitBufferSize)
        {
            _straightDelegate = new StraightProjectileArchetype(hitBufferSize);
        }

        public void Simulate(ref ProjectileInstance projectile, float dt, IProjectileContext context)
        {
            AcquireOrRetarget(ref projectile, context);

            if (projectile.TargetEntityId >= 0 && context.TargetProvider.TryGetTarget(projectile.TargetEntityId, out var target) && target.IsAlive)
            {
                var desiredDirection = (target.Position - projectile.Position).normalized;
                var maxRadians = projectile.Definition.TurnRate * Mathf.Deg2Rad * dt;
                projectile.Direction = Vector3.RotateTowards(projectile.Direction, desiredDirection, maxRadians, 0f).normalized;
            }

            _straightDelegate.Simulate(ref projectile, dt, context);
        }

        private static void AcquireOrRetarget(ref ProjectileInstance projectile, IProjectileContext context)
        {
            var mustAcquire = projectile.TargetEntityId < 0 || !context.TargetProvider.TryGetTarget(projectile.TargetEntityId, out var target) || !target.IsAlive;
            var mustRetarget = context.TimeNow >= projectile.NextRetargetTime;

            if (!mustAcquire && !mustRetarget)
            {
                return;
            }

            projectile.TargetEntityId = context.TargetProvider.AcquireTarget(
                projectile.Position,
                projectile.Direction,
                projectile.Definition.AcquireRadius,
                projectile.Definition.PreferForwardAngle,
                projectile.TeamId);

            projectile.NextRetargetTime = context.TimeNow + projectile.Definition.RetargetInterval;
        }
    }
}
