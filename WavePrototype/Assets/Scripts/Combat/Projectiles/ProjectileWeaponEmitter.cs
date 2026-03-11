using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    public sealed class ProjectileWeaponEmitter : MonoBehaviour
    {
        [SerializeField] private ProjectileSystem projectileSystem;
        [SerializeField] private ProjectileDefinition projectileDefinition;
        [SerializeField] private int ownerEntityId;
        [SerializeField] private int teamId;

        public void Fire()
        {
            if (projectileSystem == null || projectileDefinition == null)
            {
                return;
            }

            var spawn = new ProjectileSpawnContext(
                projectileDefinition,
                ownerEntityId,
                teamId,
                transform.position,
                transform.forward);

            projectileSystem.TrySpawn(spawn);
        }
    }
}
