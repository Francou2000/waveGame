using UnityEngine;
using WaveGame.Combat.Projectiles;

namespace WaveGame.Combat.Damage
{
    public sealed class DamageableBehaviour : MonoBehaviour, IDamageable
    {
        [SerializeField] private int entityId;
        [SerializeField] private int teamId;
        [SerializeField] private float health = 100f;

        public int EntityId => entityId;
        public int TeamId => teamId;
        public bool IsAlive => health > 0f;
        public Vector3 Position => transform.position;

        public void ApplyDamage(float amount, DamageType damageType, int ownerId, bool isCritical, float knockbackForce, Vector3 hitPoint, Vector3 hitNormal)
        {
            health -= amount;
            if (health <= 0f)
            {
                health = 0f;
            }
        }
    }
}
