using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WaveGame.Combat.Heat;

namespace WaveGame.Combat.UI
{
    public sealed class HeatHudView : MonoBehaviour
    {
        [SerializeField] private HeatSystem heatSystem;
        [SerializeField] private Slider heatSlider;
        [SerializeField] private TMP_Text heatLabel;
        [SerializeField] private GameObject overheatBanner;

        private void Awake()
        {
            if (heatSystem == null)
            {
                heatSystem = FindFirstObjectByType<HeatSystem>();
            }
        }

        private void OnEnable()
        {
            if (heatSystem == null)
            {
                return;
            }

            heatSystem.HeatChanged += OnHeatChanged;
            heatSystem.OverheatTriggered += OnOverheatTriggered;
            heatSystem.BossDefeated += OnBossDefeated;
            OnHeatChanged(heatSystem.Heat, heatSystem.MaxHeat);
        }

        private void OnDisable()
        {
            if (heatSystem == null)
            {
                return;
            }

            heatSystem.HeatChanged -= OnHeatChanged;
            heatSystem.OverheatTriggered -= OnOverheatTriggered;
            heatSystem.BossDefeated -= OnBossDefeated;
        }

        private void OnHeatChanged(float heat, float maxHeat)
        {
            if (heatSlider != null)
            {
                heatSlider.normalizedValue = maxHeat > 0f ? heat / maxHeat : 0f;
            }

            if (heatLabel != null)
            {
                heatLabel.text = $"HEAT {Mathf.RoundToInt(heat)}/{Mathf.RoundToInt(maxHeat)}";
            }
        }

        private void OnOverheatTriggered()
        {
            if (overheatBanner != null)
            {
                overheatBanner.SetActive(true);
            }
        }

        private void OnBossDefeated()
        {
            if (overheatBanner != null)
            {
                overheatBanner.SetActive(false);
            }
        }
    }
}
