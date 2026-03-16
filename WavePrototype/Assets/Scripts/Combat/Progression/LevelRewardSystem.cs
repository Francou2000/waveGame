using System;
using System.Collections.Generic;
using UnityEngine;
using WaveGame.Combat.Player;

namespace WaveGame.Combat.Progression
{
    public enum RewardType
    {
        Weapon,
        Passive
    }

    [Serializable]
    public sealed class RewardDefinition
    {
        public string Id = "reward.id";
        public string DisplayName = "Reward";
        [TextArea] public string Description = "";
        public RewardType Type = RewardType.Weapon;
        [Min(1)] public int MaxLevel = 5;
        [Min(0f)] public float AttackSpeedBonusPerLevel;
        [Min(0f)] public float MoveSpeedBonusPerLevel;
    }

    public sealed class LevelRewardSystem : MonoBehaviour
    {
        [SerializeField] private PlayerLevelSystem levelSystem;
        [SerializeField] private PlayerStatsRuntime playerStats;
        [SerializeField] private List<RewardDefinition> rewardPool = new();
        [SerializeField, Min(1)] private int offersPerLevel = 3;

        private readonly List<RewardDefinition> _offerBuffer = new(8);
        private readonly Dictionary<string, int> _currentLevels = new(32);

        public event Action<IReadOnlyList<RewardDefinition>> OffersReady;

        public bool TryGetRewardLevel(string rewardId, out int level)
        {
            return _currentLevels.TryGetValue(rewardId, out level);
        }

        private void Awake()
        {
            if (levelSystem == null)
            {
                levelSystem = GetComponent<PlayerLevelSystem>();
            }

            if (playerStats == null)
            {
                playerStats = GetComponent<PlayerStatsRuntime>();
            }
        }

        private void OnEnable()
        {
            if (levelSystem != null)
            {
                levelSystem.LevelUp += HandleLevelUp;
            }
        }

        private void OnDisable()
        {
            if (levelSystem != null)
            {
                levelSystem.LevelUp -= HandleLevelUp;
            }
        }

        private void HandleLevelUp(int _)
        {
            BuildOffers();
            if (_offerBuffer.Count > 0)
            {
                OffersReady?.Invoke(_offerBuffer);
            }
        }

        public void ApplyReward(RewardDefinition reward)
        {
            if (reward == null || string.IsNullOrWhiteSpace(reward.Id))
            {
                return;
            }

            _currentLevels.TryGetValue(reward.Id, out var current);
            if (current >= reward.MaxLevel)
            {
                return;
            }

            _currentLevels[reward.Id] = current + 1;

            // Lightweight stat application for vertical slice rewards.
            if (playerStats != null)
            {
                playerStats.AddAttackSpeedMultiplier(reward.AttackSpeedBonusPerLevel);
                playerStats.AddMoveSpeed(reward.MoveSpeedBonusPerLevel);
            }
        }

        private void BuildOffers()
        {
            _offerBuffer.Clear();

            for (var i = 0; i < rewardPool.Count; i++)
            {
                var reward = rewardPool[i];
                if (reward == null || string.IsNullOrWhiteSpace(reward.Id))
                {
                    continue;
                }

                _currentLevels.TryGetValue(reward.Id, out var current);
                if (current >= Mathf.Max(1, reward.MaxLevel))
                {
                    continue;
                }

                _offerBuffer.Add(reward);
            }

            if (_offerBuffer.Count <= offersPerLevel)
            {
                return;
            }

            for (var i = _offerBuffer.Count - 1; i > 0; i--)
            {
                var swap = UnityEngine.Random.Range(0, i + 1);
                (_offerBuffer[i], _offerBuffer[swap]) = (_offerBuffer[swap], _offerBuffer[i]);
            }

            _offerBuffer.RemoveRange(offersPerLevel, _offerBuffer.Count - offersPerLevel);
        }
    }
}
