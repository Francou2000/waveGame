using UnityEngine;
using WaveGame.Combat.Enemy;
using WaveGame.Combat.Heat;

namespace WaveGame.Combat.Boss
{
    public sealed class SimpleBossEncounter : MonoBehaviour
    {
        [SerializeField] private EnemyRuntime bossPrefab;
        [SerializeField] private EnemyDefinitionSO bossDefinition;
        [SerializeField] private EnemySystem enemySystem;
        [SerializeField] private EnemyDeathSystem deathSystem;
        [SerializeField] private HeatSystem heatSystem;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform playerTarget;
        [SerializeField, Min(0f)] private float overheatBonusXp = 25f;
        [SerializeField] private WaveGame.Combat.Player.PlayerStatsRuntime playerStats;

        private EnemyRuntime _activeBoss;

        public EnemyRuntime ActiveBoss => _activeBoss;

        private void Awake()
        {
            if (enemySystem == null)
            {
                enemySystem = FindFirstObjectByType<EnemySystem>();
            }

            if (deathSystem == null)
            {
                deathSystem = FindFirstObjectByType<EnemyDeathSystem>();
            }

            if (heatSystem == null)
            {
                heatSystem = FindFirstObjectByType<HeatSystem>();
            }

            if (playerStats == null)
            {
                playerStats = FindFirstObjectByType<WaveGame.Combat.Player.PlayerStatsRuntime>();
            }

            if (playerTarget == null)
            {
                var anchor = FindFirstObjectByType<WaveGame.Combat.Player.PlayerCombatAnchorProvider>();
                if (anchor != null)
                {
                    playerTarget = anchor.transform;
                }
            }
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

        public void BeginEncounter()
        {
            if (_activeBoss != null || bossPrefab == null)
            {
                return;
            }

            var position = spawnPoint != null ? spawnPoint.position : transform.position;
            _activeBoss = Instantiate(bossPrefab, position, Quaternion.identity);
            if (bossDefinition != null)
            {
                _activeBoss.SetDefinition(bossDefinition);
            }

            _activeBoss.Activate(position);
            if (enemySystem != null)
            {
                enemySystem.Register(_activeBoss);
                if (playerTarget != null)
                {
                    enemySystem.SetPlayerTarget(playerTarget);
                }
            }

            if (deathSystem != null)
            {
                deathSystem.Register(_activeBoss);
            }
        }

        private void HandleEnemyDied(EnemyRuntime enemy)
        {
            if (enemy == null || _activeBoss == null || enemy != _activeBoss)
            {
                return;
            }

            if (playerStats != null && overheatBonusXp > 0f)
            {
                playerStats.AddXp(overheatBonusXp);
            }

            if (heatSystem != null)
            {
                heatSystem.CompleteBossFight();
            }

            _activeBoss = null;
        }
    }
}
