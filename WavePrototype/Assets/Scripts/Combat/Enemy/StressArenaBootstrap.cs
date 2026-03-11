using UnityEngine;

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
                var player = FindFirstObjectByType<WaveGame.Combat.Player.PlayerCombatAnchorProvider>();
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
        }
    }
}
