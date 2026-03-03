using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    public readonly struct ProjectileSpawnContext
    {
        public readonly ProjectileDefinition Definition;
        public readonly int OwnerEntityId;
        public readonly int TeamId;
        public readonly Vector3 Position;
        public readonly Vector3 Direction;

        public ProjectileSpawnContext(ProjectileDefinition definition, int ownerEntityId, int teamId, Vector3 position, Vector3 direction)
        {
            Definition = definition;
            OwnerEntityId = ownerEntityId;
            TeamId = teamId;
            Position = position;
            Direction = direction;
        }
    }
}
