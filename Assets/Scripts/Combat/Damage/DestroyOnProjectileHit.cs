using UnityEngine;

namespace WaveGame.Combat.Damage
{
    /// <summary>
    /// Enemigo simple para prototipo: se destruye con el primer impacto de proyectil.
    /// </summary>
    public sealed class DestroyOnProjectileHit : MonoBehaviour, IDamageable
    {
        [SerializeField] private int entityId;
        [SerializeField] private int teamId = 2;
        [SerializeField] private float aimPointHeightOffset = 1f;

        public int EntityId => entityId != 0 ? entityId : gameObject.GetInstanceID();
        public int TeamId => teamId;
        public bool IsAlive => this != null && gameObject.activeInHierarchy;
        public Vector3 Position => transform.position;

        public Vector3 GetAimPoint()
        {
            return transform.position + (Vector3.up * aimPointHeightOffset);
        }

        public void ApplyDamage(DamageEvent damageEvent)
        {
            Destroy(gameObject);
        }
    }
}
