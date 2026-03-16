using UnityEngine;

namespace WaveGame.Meta
{
    public sealed class MetaUnlockState : MonoBehaviour
    {
        private const string CurrencyKey = "wave.meta.currency";
        private const string StarterUnlockKey = "wave.unlock.starter_weapon";

        public int Currency => PlayerPrefs.GetInt(CurrencyKey, 0);
        public bool StarterWeaponUnlocked => PlayerPrefs.GetInt(StarterUnlockKey, 0) == 1;

        public void AddCurrency(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            PlayerPrefs.SetInt(CurrencyKey, Currency + amount);
            PlayerPrefs.Save();
        }

        public void UnlockStarterWeapon()
        {
            PlayerPrefs.SetInt(StarterUnlockKey, 1);
            PlayerPrefs.Save();
        }
    }
}
