using System.Collections.Generic;
using UnityEngine;
using WaveGame.Combat.Player;

namespace WaveGame.Combat.Enemy
{
    public sealed class XpOrbSystem : MonoBehaviour
    {
        [SerializeField] private XpOrbRuntime orbPrefab;
        [SerializeField] private PlayerStatsRuntime playerStats;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private int initialPoolSize = 128;
        [SerializeField] private int maxActiveOrbs = 600;
        [SerializeField] private float magnetRadius = 4f;
        [SerializeField] private float attractSpeed = 12f;
        [SerializeField] private float pickupDistance = 0.6f;
        [SerializeField] private float mergeRadius = 1.1f;

        private readonly Queue<XpOrbRuntime> _pool = new();
        private readonly List<XpOrbRuntime> _active = new(1024);

        private void Awake()
        {
            if (playerTransform == null)
            {
                var anchor = FindFirstObjectByType<PlayerCombatAnchorProvider>();
                if (anchor != null)
                {
                    playerTransform = anchor.transform;
                }
            }

            WarmPool(Mathf.Max(1, initialPoolSize));
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                return;
            }

            var dt = Time.deltaTime;
            var playerPos = playerTransform.position;

            for (var i = _active.Count - 1; i >= 0; i--)
            {
                var orb = _active[i];
                if (orb == null || !orb.IsActive)
                {
                    ReturnAt(i);
                    continue;
                }

                var toPlayer = playerPos - orb.transform.position;
                var sqrDist = toPlayer.sqrMagnitude;
                if (sqrDist <= magnetRadius * magnetRadius)
                {
                    if (sqrDist <= pickupDistance * pickupDistance)
                    {
                        if (playerStats != null)
                        {
                            playerStats.AddXp(orb.Value);
                        }

                        ReturnAt(i);
                        continue;
                    }

                    var dir = toPlayer.normalized;
                    orb.transform.position += dir * attractSpeed * dt;
                }
            }
        }

        public void SpawnXp(Vector3 position, float xpValue)
        {
            if (xpValue <= 0f)
            {
                return;
            }

            if (TryMergeNearby(position, xpValue))
            {
                return;
            }

            if (_active.Count >= maxActiveOrbs)
            {
                return;
            }

            if (_pool.Count == 0)
            {
                WarmPool(32);
            }

            var orb = _pool.Dequeue();
            orb.Activate(position, xpValue);
            _active.Add(orb);
        }

        private bool TryMergeNearby(Vector3 position, float xpValue)
        {
            var best = (XpOrbRuntime)null;
            var bestSqr = mergeRadius * mergeRadius;

            for (var i = 0; i < _active.Count; i++)
            {
                var orb = _active[i];
                if (orb == null || !orb.IsActive)
                {
                    continue;
                }

                var sqr = (orb.transform.position - position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = orb;
                }
            }

            if (best == null)
            {
                return false;
            }

            best.AddValue(xpValue);
            return true;
        }

        private void WarmPool(int amount)
        {
            if (orbPrefab == null)
            {
                return;
            }

            for (var i = 0; i < amount; i++)
            {
                var orb = Instantiate(orbPrefab, transform);
                orb.gameObject.SetActive(false);
                _pool.Enqueue(orb);
            }
        }

        private void ReturnAt(int index)
        {
            var orb = _active[index];
            _active.RemoveAt(index);
            if (orb == null)
            {
                return;
            }

            orb.Deactivate();
            _pool.Enqueue(orb);
        }
    }
}
