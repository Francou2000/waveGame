using UnityEngine;
using WaveGame.Combat.Enemy;

namespace WaveGame.Combat.Heat
{
    public sealed class EnemyDirector : MonoBehaviour
    {
        [SerializeField] private HeatSystem heatSystem;
        [SerializeField] private EnemySpawner minionSpawner;
        [SerializeField] private EnemySpawner eliteSpawner;
        [SerializeField] private WaveGame.Combat.Boss.SimpleBossEncounter bossEncounter;
        [SerializeField, Min(1)] private int baseMaxAlive = 300;
        [SerializeField, Min(0f)] private float baseSpawnRate = 20f;
        [SerializeField, Min(1f)] private float baseSpawnRadius = 30f;
        [SerializeField, Min(1)] private int surgeBonusAlive = 120;

        private void Awake()
        {
            if (heatSystem == null)
            {
                heatSystem = FindFirstObjectByType<HeatSystem>();
            }

            if (minionSpawner == null)
            {
                minionSpawner = FindFirstObjectByType<EnemySpawner>();
            }

            if (bossEncounter == null)
            {
                bossEncounter = FindFirstObjectByType<WaveGame.Combat.Boss.SimpleBossEncounter>();
            }
        }

        private void OnEnable()
        {
            if (heatSystem == null)
            {
                return;
            }

            heatSystem.HeatChanged += HandleHeatChanged;
            heatSystem.OverheatTriggered += HandleOverheatTriggered;
            heatSystem.BossDefeated += HandleBossDefeated;
            HandleHeatChanged(heatSystem.Heat, heatSystem.MaxHeat);
        }

        private void OnDisable()
        {
            if (heatSystem == null)
            {
                return;
            }

            heatSystem.HeatChanged -= HandleHeatChanged;
            heatSystem.OverheatTriggered -= HandleOverheatTriggered;
            heatSystem.BossDefeated -= HandleBossDefeated;
        }

        private void HandleHeatChanged(float _, float __)
        {
            if (heatSystem == null || minionSpawner == null)
            {
                return;
            }

            var tier = heatSystem.EvaluateCurrentTier();
            var spawnRate = baseSpawnRate * Mathf.Max(0.1f, tier.SpawnRateMultiplier);
            var maxAlive = baseMaxAlive + Mathf.RoundToInt(baseMaxAlive * heatSystem.NormalizedHeat * 0.4f);
            minionSpawner.ConfigureRuntime(maxAlive, spawnRate, baseSpawnRadius);

            if (eliteSpawner != null)
            {
                var eliteRate = spawnRate * tier.EliteChance * 0.6f;
                eliteSpawner.ConfigureRuntime(Mathf.Max(20, maxAlive / 5), eliteRate, baseSpawnRadius * 0.95f);
            }
        }

        private void HandleOverheatTriggered()
        {
            if (minionSpawner != null)
            {
                minionSpawner.ConfigureRuntime(baseMaxAlive + surgeBonusAlive, baseSpawnRate * 1.8f, baseSpawnRadius);
            }

            if (eliteSpawner != null)
            {
                eliteSpawner.ConfigureRuntime(100, baseSpawnRate * 0.35f, baseSpawnRadius * 0.9f);
            }

            if (bossEncounter != null)
            {
                bossEncounter.BeginEncounter();
                heatSystem.SetBossFightActive();
            }
        }

        private void HandleBossDefeated()
        {
            HandleHeatChanged(heatSystem.Heat, heatSystem.MaxHeat);
        }
    }
}
