using System;
using UnityEngine;

namespace WaveGame.Meta.Save
{
    public static class MetaProgressionSaveStore
    {
        private const string SaveKey = "wavegame.meta.progression.v1";

        public static MetaProgressionSaveData Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return new MetaProgressionSaveData();
            }

            try
            {
                var json = PlayerPrefs.GetString(SaveKey, string.Empty);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new MetaProgressionSaveData();
                }

                var data = JsonUtility.FromJson<MetaProgressionSaveData>(json);
                return data ?? new MetaProgressionSaveData();
            }
            catch (Exception)
            {
                return new MetaProgressionSaveData();
            }
        }

        public static void Save(MetaProgressionSaveData data)
        {
            if (data == null)
            {
                return;
            }

            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }
}
