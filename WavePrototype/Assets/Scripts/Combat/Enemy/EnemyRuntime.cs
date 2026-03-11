using System;
using UnityEngine;
using WaveGame.Combat.Damage;

namespace WaveGame.Combat.Enemy
{
    public sealed class EnemyRuntime : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyDefinitionSO definition;
        [SerializeField] private int entityId;
        [SerializeField] private int teamId = 2;
        [SerializeField] private float maxHealth = 10f;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float aimPointHeightOffset = 1f;
        [SerializeField] private Renderer visualRenderer;

        private float _currentHealth;
        private bool _isDying;

        public event Action<EnemyRuntime> Died;
        public event Action<EnemyRuntime> DeathRequested;

        public int EntityId => entityId != 0 ? entityId : gameObject.GetInstanceID();
        public int TeamId => teamId;
        public bool IsAlive => _currentHealth > 0f && !_isDying && gameObject.activeInHierarchy;
        public Vector3 Position => transform.position;
        public float MoveSpeed => moveSpeed;
        public EnemyCategory Category => definition != null ? definition.Category : EnemyCategory.Minion;

        public void SetDefinition(EnemyDefinitionSO enemyDefinition)
        {
            definition = enemyDefinition;
            if (definition == null)
            {
                return;
            }

            maxHealth = definition.MaxHp;
            moveSpeed = definition.MoveSpeed;
            aimPointHeightOffset = definition.AimPointOffset;
            transform.localScale = definition.ModelScale;
        }

        public void Activate(Vector3 position)
        {
            if (definition != null)
            {
                SetDefinition(definition);
                ApplyVisualVariant();
            }

            transform.position = position;
            _currentHealth = maxHealth;
            _isDying = false;
            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            if (_currentHealth <= 0f)
            {
                _currentHealth = maxHealth;
            }

            _isDying = false;
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

            var reduction = definition != null ? definition.DamageReduction : 0f;
            var finalDamage = damageEvent.Amount * (1f - Mathf.Clamp01(reduction));
            _currentHealth -= finalDamage;
            if (_currentHealth > 0f)
            {
                return;
            }

            _currentHealth = 0f;
            _isDying = true;

            if (TryGetComponent<Collider>(out var col))
            {
                col.enabled = false;
            }

            var handler = DeathRequested;
            if (handler != null)
            {
                handler.Invoke(this);
                return;
            }

            FinalizeDeath();
        }

        public float EvaluateXpDrop()
        {
            if (definition == null || !definition.DropOnDeath)
            {
                return 0f;
            }

            if (definition.DropTable == null)
            {
                return Mathf.Max(0f, definition.XpValue);
            }

            return definition.DropTable.EvaluateXp(definition.XpValue);
        }

        public void FinalizeDeath()
        {
            if (TryGetComponent<Collider>(out var col))
            {
                col.enabled = true;
            }

            Died?.Invoke(this);
            gameObject.SetActive(false);
        }

        private void ApplyVisualVariant()
        {
            if (definition == null || definition.VisualSet == null || visualRenderer == null)
            {
                return;
            }

            var visualSet = definition.VisualSet;
            var pickedMaterial = visualSet.PickMaterial();
            if (pickedMaterial != null)
            {
                visualRenderer.sharedMaterial = pickedMaterial;
            }

            visualRenderer.material.color = visualSet.PickTint();
            transform.localScale = definition.ModelScale * visualSet.PickScaleMultiplier();
        }
    }
}
