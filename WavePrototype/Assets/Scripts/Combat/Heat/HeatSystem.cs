using System;
using UnityEngine;
using WaveGame.Combat.Enemy;

namespace WaveGame.Combat.Heat
{
    public enum OverheatPhase
    {
        Idle,
        Surge,
        BossFight
    }

    [Serializable]
    public struct HeatTier
    {
        [Range(0f, 1f)] public float MinNormalizedHeat;
        [Min(0f)] public float SpawnRateMultiplier;
        [Range(0f, 1f)] public float EliteChance;
    }

    public sealed class HeatSystem : MonoBehaviour
    {
        [SerializeField] private EnemyDeathSystem deathSystem;
        [SerializeField, Min(1f)] private float maxHeat = 100f;
        [SerializeField, Min(0f)] private float baselineAfterBoss = 30f;
        [SerializeField, Min(0f)] private float heatPerKill = 1f;
        [SerializeField] private HeatTier[] tiers;

        private float _heat;
        private OverheatPhase _phase;

        public event Action<float, float> HeatChanged;
        public event Action OverheatTriggered;
        public event Action BossDefeated;

        public float Heat => _heat;
        public float MaxHeat => maxHeat;
        public float NormalizedHeat => Mathf.Clamp01(_heat / Mathf.Max(1f, maxHeat));
        public OverheatPhase Phase => _phase;

        private void Awake()
        {
            if (deathSystem == null)
            {
                deathSystem = FindFirstObjectByType<EnemyDeathSystem>();
            }

            _heat = baselineAfterBoss;
            NotifyHeatChanged();
        }

        private void OnEnable()
        {
            if (deathSystem != null)
            {
                deathSystem.EnemyDied += HandleEnemyDied;
            }
        }

        private void OnDisable()
        {
            if (deathSystem != null)
            {
                deathSystem.EnemyDied -= HandleEnemyDied;
            }
        }

        private void HandleEnemyDied(EnemyRuntime enemy)
        {
            if (enemy == null || enemy.Category == EnemyCategory.Boss)
            {
                return;
            }

            AddHeat(heatPerKill);
        }

        public void AddHeat(float amount)
        {
            if (amount <= 0f || _phase != OverheatPhase.Idle)
            {
                return;
            }

            _heat = Mathf.Min(maxHeat, _heat + amount);
            NotifyHeatChanged();

            if (_heat >= maxHeat)
            {
                _phase = OverheatPhase.Surge;
                OverheatTriggered?.Invoke();
            }
        }

        public HeatTier EvaluateCurrentTier()
        {
            var normalized = NormalizedHeat;
            var selected = new HeatTier { MinNormalizedHeat = 0f, SpawnRateMultiplier = 1f, EliteChance = 0f };
            if (tiers == null)
            {
                return selected;
            }

            for (var i = 0; i < tiers.Length; i++)
            {
                if (normalized >= tiers[i].MinNormalizedHeat)
                {
                    selected = tiers[i];
                }
            }

            return selected;
        }

        public void SetBossFightActive()
        {
            _phase = OverheatPhase.BossFight;
        }

        public void CompleteBossFight()
        {
            _phase = OverheatPhase.Idle;
            _heat = Mathf.Clamp(baselineAfterBoss, 0f, maxHeat);
            NotifyHeatChanged();
            BossDefeated?.Invoke();
        }

        private void NotifyHeatChanged()
        {
            HeatChanged?.Invoke(_heat, maxHeat);
        }
    }
}
