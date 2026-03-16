using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WaveGame.Combat.Boss;

namespace WaveGame.Combat.UI
{
    public sealed class BossHealthHudView : MonoBehaviour
    {
        [SerializeField] private SimpleBossEncounter encounter;
        [SerializeField] private GameObject root;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TMP_Text label;

        private void Awake()
        {
            if (encounter == null)
            {
                encounter = FindFirstObjectByType<SimpleBossEncounter>();
            }
        }

        private void Update()
        {
            var boss = encounter != null ? encounter.ActiveBoss : null;
            var hasBoss = boss != null && boss.gameObject.activeInHierarchy;

            if (root != null)
            {
                root.SetActive(hasBoss);
            }

            if (!hasBoss)
            {
                return;
            }

            if (label != null)
            {
                label.text = "BOSS";
            }

            if (healthSlider != null)
            {
                healthSlider.normalizedValue = boss.CurrentHealth / Mathf.Max(1f, boss.MaxHealth);
            }
        }
    }
}
