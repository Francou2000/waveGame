using System;
using UnityEngine;
using WaveGame.Combat.Damage;

namespace WaveGame.Combat.Enemy
{
    public sealed class EnemyRuntime : MonoBehaviour, IDamageable
    {
        [SerializeField] private int entityId;
        [SerializeField] private int teamId = 2;
        [SerializeField] private float maxHealth = 10f;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float aimPointHeightOffset = 1f;

        private float _currentHealth;

        public event Action<EnemyRuntime> Died;

        public int EntityId => entityId != 0 ? entityId : gameObject.GetInstanceID();
        public int TeamId => teamId;
        public bool IsAlive => _currentHealth > 0f && gameObject.activeInHierarchy;
        public Vector3 Position => transform.position;
        public float MoveSpeed => moveSpeed;

        public void Activate(Vector3 position)
        {
            transform.position = position;
            _currentHealth = maxHealth;
            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            if (_currentHealth <= 0f)
            {
                _currentHealth = maxHealth;
            }
        }

        public Vector3 GetAimPoint()
        {
            return transform.position + Vector3.up * aimPointHeightOffset;
        }

        public void ApplyDamage(DamageEvent damageEvent)
        {
            if (!IsAlive)
            {
                return;
            }

            _currentHealth -= damageEvent.Amount;
            if (_currentHealth > 0f)
            {
                return;
            }

            _currentHealth = 0f;
            Died?.Invoke(this);
            gameObject.SetActive(false);
        }
    }
}
