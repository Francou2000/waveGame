using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WaveGame.Meta.Definitions;
using WaveGame.Meta.Save;

namespace WaveGame.Meta.Runtime
{
    public sealed class MetaProgressionManager : MonoBehaviour
    {
        [SerializeField] private ContentCatalogSO contentCatalog;
        [SerializeField] private bool autoUnlockAllInEditor;

        private readonly HashSet<string> _unlocked = new();
        private readonly HashSet<string> _discovered = new();
        private readonly HashSet<string> _systemUnlocks = new();

        private MetaProgressionSaveData _save;

        public int MetaCurrency => _save?.MetaCurrency ?? 0;

        private void Awake()
        {
            _save = MetaProgressionSaveStore.Load();
            SyncSetsFromSave();

#if UNITY_EDITOR
            if (autoUnlockAllInEditor && contentCatalog != null)
            {
                UnlockAllCatalogContent();
            }
#endif
        }

        public RunSessionState BuildRunSessionState(int seed)
        {
            var run = new RunSessionState { Seed = seed };
            if (contentCatalog == null)
            {
                return run;
            }

            foreach (var weapon in contentCatalog.Weapons)
            {
                if (weapon == null || !_unlocked.Contains(weapon.ContentId))
                {
                    continue;
                }

                run.AvailableWeaponIds.Add(weapon.ContentId);
            }

            foreach (var passive in contentCatalog.Passives)
            {
                if (passive == null || !_unlocked.Contains(passive.ContentId))
                {
                    continue;
                }

                run.AvailablePassiveIds.Add(passive.ContentId);
            }

            return run;
        }

        public bool IsUnlocked(string contentId)
        {
            return !string.IsNullOrEmpty(contentId) && _unlocked.Contains(contentId);
        }

        public void UnlockContent(string contentId)
        {
            if (string.IsNullOrEmpty(contentId) || !_unlocked.Add(contentId))
            {
                return;
            }

            if (!_save.UnlockedContentIds.Contains(contentId))
            {
                _save.UnlockedContentIds.Add(contentId);
            }

            MetaProgressionSaveStore.Save(_save);
        }

        public void AddMetaCurrency(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _save.MetaCurrency += amount;
            MetaProgressionSaveStore.Save(_save);
        }

        public void BanishFromRun(RunSessionState runState, string contentId)
        {
            if (runState == null || string.IsNullOrEmpty(contentId))
            {
                return;
            }

            runState.BannedIds.Add(contentId);
            runState.AvailableWeaponIds.Remove(contentId);
            runState.AvailablePassiveIds.Remove(contentId);
        }

        public void MarkTakenInRun(RunSessionState runState, string contentId)
        {
            if (runState == null || string.IsNullOrEmpty(contentId))
            {
                return;
            }

            runState.TakenThisRun.Add(contentId);
            runState.AvailableWeaponIds.Remove(contentId);
            runState.AvailablePassiveIds.Remove(contentId);
        }

        public bool TryGetEligibleEvolution(string baseWeaponId, int baseWeaponLevel, HashSet<string> takenPassives, EvolutionTriggerSource triggerSource, out EvolutionRecipeSO recipe)
        {
            recipe = null;
            if (contentCatalog == null)
            {
                return false;
            }

            foreach (var candidate in contentCatalog.EvolutionRecipes)
            {
                if (candidate == null)
                {
                    continue;
                }

                if (!string.Equals(candidate.BaseWeaponId, baseWeaponId))
                {
                    continue;
                }

                if (baseWeaponLevel < candidate.MinBaseWeaponLevel)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(candidate.RequiredPassiveId) && (takenPassives == null || !takenPassives.Contains(candidate.RequiredPassiveId)))
                {
                    continue;
                }

                if (candidate.TriggerSource != EvolutionTriggerSource.Any && candidate.TriggerSource != triggerSource)
                {
                    continue;
                }

                if (!_unlocked.Contains(candidate.ResultWeaponId))
                {
                    continue;
                }

                recipe = candidate;
                return true;
            }

            return false;
        }

        private void SyncSetsFromSave()
        {
            _unlocked.Clear();
            _discovered.Clear();
            _systemUnlocks.Clear();

            foreach (var id in _save.UnlockedContentIds)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    _unlocked.Add(id);
                }
            }

            foreach (var id in _save.DiscoveredContentIds)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    _discovered.Add(id);
                }
            }

            foreach (var id in _save.SystemUnlockIds)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    _systemUnlocks.Add(id);
                }
            }
        }

        private void UnlockAllCatalogContent()
        {
            if (contentCatalog == null)
            {
                return;
            }

            var allIds = contentCatalog.Weapons.Where(w => w != null).Select(w => w.ContentId)
                .Concat(contentCatalog.Passives.Where(p => p != null).Select(p => p.ContentId))
                .Concat(contentCatalog.Abilities.Where(a => a != null).Select(a => a.ContentId));

            foreach (var id in allIds)
            {
                UnlockContent(id);
            }
        }
    }
}
