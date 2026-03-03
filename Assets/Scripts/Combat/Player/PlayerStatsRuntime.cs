using UnityEngine;

namespace WaveGame.Combat.Player
{
    public sealed class PlayerStatsRuntime : MonoBehaviour
    {
        [SerializeField] private PlayerStatsDefinitionSO definition;

        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float attackSpeedMultiplier = 1f;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float MoveSpeed => moveSpeed;
        public float AttackSpeedMultiplier => attackSpeedMultiplier;

        private void Awake()
        {
            if (definition == null)
            {
                return;
            }

            maxHealth = definition.MaxHealth;
            currentHealth = maxHealth;
            moveSpeed = definition.MoveSpeed;
            attackSpeedMultiplier = definition.AttackSpeedMultiplier;
        }

        public void ApplyDamage(float amount)
        {
            currentHealth = Mathf.Max(0f, currentHealth - amount);
        }
    }
}
