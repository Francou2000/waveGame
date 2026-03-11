using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    /// <summary>
    /// Spawner simple para escena de testing: instancia enemigos estáticos en un anillo.
    /// </summary>
    public sealed class TestingArenaSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private int enemyCount = 24;
        [SerializeField] private float radius = 14f;

        private void Start()
        {
            if (enemyPrefab == null)
            {
                return;
            }

            var count = Mathf.Max(1, enemyCount);
            for (var i = 0; i < count; i++)
            {
                var angle = i / (float)count * Mathf.PI * 2f;
                var position = transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                Instantiate(enemyPrefab, position, Quaternion.identity, transform);
            }
        }
    }
}
