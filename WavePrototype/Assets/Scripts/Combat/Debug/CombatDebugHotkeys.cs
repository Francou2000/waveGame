using UnityEngine;
using WaveGame.Combat.Boss;
using WaveGame.Combat.Enemy;
using WaveGame.Combat.Heat;
using WaveGame.Combat.Player;

namespace WaveGame.Combat.DebugTools
{
    public sealed class CombatDebugHotkeys : MonoBehaviour
    {
        [SerializeField] private HeatSystem heatSystem;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private SimpleBossEncounter bossEncounter;
        [SerializeField] private PlayerStatsRuntime playerStats;

        [Header("Hotkeys")]
        [SerializeField] private KeyCode addHeatKey = KeyCode.F1;
        [SerializeField] private KeyCode overheatKey = KeyCode.F2;
        [SerializeField] private KeyCode bossKey = KeyCode.F3;
        [SerializeField] private KeyCode grantXpKey = KeyCode.F4;
        [SerializeField] private KeyCode spawnBurstKey = KeyCode.F5;

        private void Awake()
        {
            if (heatSystem == null) heatSystem = FindFirstObjectByType<HeatSystem>();
            if (enemySpawner == null) enemySpawner = FindFirstObjectByType<EnemySpawner>();
            if (bossEncounter == null) bossEncounter = FindFirstObjectByType<SimpleBossEncounter>();
            if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStatsRuntime>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(addHeatKey))
            {
                heatSystem?.AddHeat(10f);
            }

            if (Input.GetKeyDown(overheatKey) && heatSystem != null)
            {
                heatSystem.AddHeat(heatSystem.MaxHeat);
            }

            if (Input.GetKeyDown(bossKey))
            {
                bossEncounter?.BeginEncounter();
                heatSystem?.SetBossFightActive();
            }

            if (Input.GetKeyDown(grantXpKey))
            {
                playerStats?.AddXp(25f);
            }

            if (Input.GetKeyDown(spawnBurstKey) && enemySpawner != null)
            {
                // Temporary spike for stress checks.
                enemySpawner.ConfigureRuntime(1000, 250f, 30f);
            }
        }
    }
}
