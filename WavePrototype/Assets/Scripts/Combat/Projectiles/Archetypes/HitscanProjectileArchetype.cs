namespace WaveGame.Combat.Projectiles.Archetypes
{
    public sealed class HitscanProjectileArchetype : IProjectileArchetype
    {
        private readonly StraightProjectileArchetype _delegate;

        public HitscanProjectileArchetype(int hitBufferSize)
        {
            _delegate = new StraightProjectileArchetype(hitBufferSize);
        }

        public void Simulate(ref ProjectileInstance projectile, float dt, IProjectileContext context)
        {
            var castDt = projectile.Speed > 0f ? projectile.Definition.MaxDistance / projectile.Speed : 0f;
            _delegate.Simulate(ref projectile, castDt, context);
            projectile.IsFinished = true;
        }
    }
}
