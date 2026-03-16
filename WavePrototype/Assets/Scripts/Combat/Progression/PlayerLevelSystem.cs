using System;
using UnityEngine;
using WaveGame.Combat.Player;

namespace WaveGame.Combat.Progression
{
    public sealed class PlayerLevelSystem : MonoBehaviour
    {
        [SerializeField] private PlayerStatsRuntime playerStats;
        [SerializeField, Min(1)] private int startLevel = 1;
        [SerializeField, Min(1f)] private float xpBaseRequirement = 10f;
        [SerializeField, Min(0f)] private float xpGrowthPerLevel = 3f;

        private int _level;
        private float _trackedXp;
        private float _spentXp;

        public event Action<int> LevelUp;

        public int Level => _level;
        public float CurrentXpInLevel => Mathf.Max(0f, _trackedXp - _spentXp);
        public float CurrentLevelRequirement => GetRequirementForLevel(_level);

        private void Awake()
        {
            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStatsRuntime>();
            }

            _level = Mathf.Max(1, startLevel);
        }

        private void Update()
        {
            if (playerStats == null)
            {
                return;
            }

            _trackedXp = playerStats.CurrentXp;
            var leveled = false;
            var guard = 0;
            while (CurrentXpInLevel >= CurrentLevelRequirement && guard++ < 16)
            {
                _spentXp += CurrentLevelRequirement;
                _level++;
                leveled = true;
                LevelUp?.Invoke(_level);
            }

            if (leveled)
            {
                // Intentionally left blank for future analytics hook.
            }
        }

        private float GetRequirementForLevel(int level)
        {
            var lv = Mathf.Max(1, level);
            return xpBaseRequirement + (lv - 1) * xpGrowthPerLevel;
        }
    }
}
