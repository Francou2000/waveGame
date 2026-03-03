using UnityEngine;
using WaveGame.Combat.Projectiles;

namespace WaveGame.Combat.Damage
{
    /// <summary>
    /// Enemigo simple para prototipo: se destruye con el primer impacto de proyectil.
    /// </summary>
    public sealed class DestroyOnProjectileHit : MonoBehaviour, IDamageable
    {
        [SerializeField] private int entityId;
        [SerializeField] private int teamId = 2;

        public int EntityId => entityId != 0 ? entityId : gameObject.GetInstanceID();
        public int TeamId => teamId;
        public bool IsAlive => this != null && gameObject.activeInHierarchy;
        public Vector3 Position => transform.position;

        public void ApplyDamage(float amount, DamageType damageType, int ownerId, bool isCritical, float knockbackForce, Vector3 hitPoint, Vector3 hitNormal)
        {
            Destroy(gameObject);
        }
    }
}
