using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    public struct ProjectileInstance
    {
        public int InstanceId;
        public ProjectileDefinition Definition;
        public int OwnerEntityId;
        public int TeamId;

        public Vector3 Position;
        public Vector3 Direction;
        public float Speed;

        public float Travelled;
        public float TimeAlive;
        public int PiercesLeft;
        public int BouncesLeft;
        public int HitsDone;

        public int TargetEntityId;
        public float NextRetargetTime;
        public float NextTickTime;

        public float DamageScale;
        public bool IsFinished;

        public static ProjectileInstance Create(int instanceId, ProjectileDefinition definition, int ownerEntityId, int teamId, Vector3 position, Vector3 direction)
        {
            return new ProjectileInstance
            {
                InstanceId = instanceId,
                Definition = definition,
                OwnerEntityId = ownerEntityId,
                TeamId = teamId,
                Position = position,
                Direction = direction.normalized,
                Speed = definition.Speed,
                PiercesLeft = definition.PierceCount,
                BouncesLeft = definition.BounceCount,
                DamageScale = 1f,
                TargetEntityId = -1
            };
        }
    }
}
