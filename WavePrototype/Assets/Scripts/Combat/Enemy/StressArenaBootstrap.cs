using UnityEngine;
using WaveGame.Combat.Boss;
using WaveGame.Combat.Heat;
using WaveGame.Combat.Player;
using WaveGame.Combat.Progression;

namespace WaveGame.Combat.Enemy
{
    /// <summary>
    /// Helper para montar rápido la stress arena desde inspector.
    /// Auto-cablea referencias y aplica valores de stress sugeridos.
    /// </summary>
    public sealed class StressArenaBootstrap : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private EnemySystem enemySystem;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private EnemyDeathSystem enemyDeathSystem;
        [SerializeField] private XpOrbSystem xpOrbSystem;
        [SerializeField] private bool applyRecommendedStressValues = true;

        [Header("Recommended Stress")]
        [SerializeField] private int maxAlive = 900;
        [SerializeField] private float spawnPerSecond = 120f;
        [SerializeField] private float spawnRadius = 28f;

        [Header("Prototype Runtime Addons")]
        [SerializeField] private HeatSystem heatSystem;
        [SerializeField] private EnemyDirector enemyDirector;
        [SerializeField] private SimpleBossEncounter bossEncounter;
        [SerializeField] private PlayerLevelSystem levelSystem;
        [SerializeField] private LevelRewardSystem rewardSystem;
        [SerializeField] private WaveGame.Combat.DebugTools.CombatDebugHotkeys debugHotkeys;

        private void Awake()
        {
            if (enemySystem == null)
            {
                enemySystem = FindFirstObjectByType<EnemySystem>();
            }

            if (enemySpawner == null)
            {
                enemySpawner = FindFirstObjectByType<EnemySpawner>();
            }

            if (enemyDeathSystem == null)
            {
                enemyDeathSystem = FindFirstObjectByType<EnemyDeathSystem>();
            }

            if (xpOrbSystem == null)
            {
                xpOrbSystem = FindFirstObjectByType<XpOrbSystem>();
            }

            if (playerTransform == null)
            {
                var player = FindFirstObjectByType<PlayerCombatAnchorProvider>();
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }

            if (enemySystem != null && playerTransform != null)
            {
                enemySystem.SetPlayerTarget(playerTransform);
            }

            if (applyRecommendedStressValues && enemySpawner != null)
            {
                enemySpawner.ConfigureRuntime(maxAlive, spawnPerSecond, spawnRadius);
            }

            EnsurePrototypeSystems();
        }

        private void EnsurePrototypeSystems()
        {
            if (heatSystem == null)
            {
                heatSystem = FindFirstObjectByType<HeatSystem>();
            }

            if (heatSystem == null)
            {
                heatSystem = gameObject.AddComponent<HeatSystem>();
            }

            if (enemyDirector == null)
            {
                enemyDirector = FindFirstObjectByType<EnemyDirector>();
            }

            if (enemyDirector == null)
            {
                enemyDirector = gameObject.AddComponent<EnemyDirector>();
            }

            if (bossEncounter == null)
            {
                bossEncounter = FindFirstObjectByType<SimpleBossEncounter>();
            }

            if (bossEncounter == null)
            {
                bossEncounter = gameObject.AddComponent<SimpleBossEncounter>();
            }

            if (levelSystem == null && playerTransform != null)
            {
                levelSystem = playerTransform.GetComponent<PlayerLevelSystem>();
                if (levelSystem == null)
                {
                    levelSystem = playerTransform.gameObject.AddComponent<PlayerLevelSystem>();
                }
            }

            if (rewardSystem == null && playerTransform != null)
            {
                rewardSystem = playerTransform.GetComponent<LevelRewardSystem>();
                if (rewardSystem == null)
                {
                    rewardSystem = playerTransform.gameObject.AddComponent<LevelRewardSystem>();
                }
            }

            if (debugHotkeys == null)
            {
                debugHotkeys = FindFirstObjectByType<WaveGame.Combat.DebugTools.CombatDebugHotkeys>();
                if (debugHotkeys == null)
                {
                    debugHotkeys = gameObject.AddComponent<WaveGame.Combat.DebugTools.CombatDebugHotkeys>();
                }
            }
        }
    }
}
