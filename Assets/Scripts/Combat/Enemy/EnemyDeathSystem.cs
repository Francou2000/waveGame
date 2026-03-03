using UnityEngine;

namespace WaveGame.Combat.Enemy
{
    public sealed class EnemyDeathSystem : MonoBehaviour
    {
        [SerializeField] private XpOrbSystem xpOrbSystem;

        public void Register(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.DeathRequested -= HandleDeathRequested;
            enemy.DeathRequested += HandleDeathRequested;
        }

        public void Unregister(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.DeathRequested -= HandleDeathRequested;
        }

        private void HandleDeathRequested(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return;
            }

            var xpValue = enemy.EvaluateXpDrop();
            if (xpOrbSystem != null && xpValue > 0f)
            {
                xpOrbSystem.SpawnXp(enemy.transform.position, xpValue);
            }

            enemy.FinalizeDeath();
        }
    }
}
