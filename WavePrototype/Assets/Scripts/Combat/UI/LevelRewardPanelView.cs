using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WaveGame.Combat.Progression;

namespace WaveGame.Combat.UI
{
    public sealed class LevelRewardPanelView : MonoBehaviour
    {
        [System.Serializable]
        private sealed class RewardButton
        {
            public Button Button;
            public TMP_Text Title;
            public TMP_Text Description;
        }

        [SerializeField] private LevelRewardSystem rewardSystem;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private RewardButton[] buttons;

        private readonly List<RewardDefinition> _currentOffers = new(3);

        private void Awake()
        {
            if (rewardSystem == null)
            {
                rewardSystem = FindFirstObjectByType<LevelRewardSystem>();
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (rewardSystem != null)
            {
                rewardSystem.OffersReady += ShowOffers;
            }
        }

        private void OnDisable()
        {
            if (rewardSystem != null)
            {
                rewardSystem.OffersReady -= ShowOffers;
            }

            BindButtons();
        }

        private void ShowOffers(IReadOnlyList<RewardDefinition> offers)
        {
            _currentOffers.Clear();
            for (var i = 0; i < offers.Count; i++)
            {
                _currentOffers.Add(offers[i]);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(_currentOffers.Count > 0);
            }

            Time.timeScale = _currentOffers.Count > 0 ? 0f : 1f;
            BindButtons();
        }

        private void BindButtons()
        {
            if (buttons == null)
            {
                return;
            }

            for (var i = 0; i < buttons.Length; i++)
            {
                var entry = buttons[i];
                if (entry == null || entry.Button == null)
                {
                    continue;
                }

                entry.Button.onClick.RemoveAllListeners();
                var hasOffer = i < _currentOffers.Count;
                entry.Button.gameObject.SetActive(hasOffer);
                if (!hasOffer)
                {
                    continue;
                }

                var offer = _currentOffers[i];
                if (entry.Title != null)
                {
                    entry.Title.text = offer.DisplayName;
                }

                if (entry.Description != null)
                {
                    entry.Description.text = offer.Description;
                }

                var capture = offer;
                entry.Button.onClick.AddListener(() => SelectReward(capture));
            }
        }

        private void SelectReward(RewardDefinition reward)
        {
            rewardSystem?.ApplyReward(reward);
            _currentOffers.Clear();
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            Time.timeScale = 1f;
        }
    }
}
