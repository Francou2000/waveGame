using UnityEngine;

namespace WaveGame.Combat.Damage
{
    public sealed class DamageableBehaviour : MonoBehaviour, IDamageable
    {
        [SerializeField] private int entityId;
        [SerializeField] private int teamId;
        [SerializeField] private float health = 100f;
        [SerializeField] private float aimPointHeightOffset = 1f;

        public int EntityId => entityId;
        public int TeamId => teamId;
        public bool IsAlive => health > 0f;
        public Vector3 Position => transform.position;

        public Vector3 GetAimPoint()
        {
            return transform.position + (Vector3.up * aimPointHeightOffset);
        }

        public void ApplyDamage(DamageEvent damageEvent)
        {
            health -= damageEvent.Amount;
            if (health <= 0f)
            {
                health = 0f;
            }
        }
    }
}
