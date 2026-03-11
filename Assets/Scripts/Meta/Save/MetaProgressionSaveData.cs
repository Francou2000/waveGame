using System;
using System.Collections.Generic;

namespace WaveGame.Meta.Save
{
    [Serializable]
    public sealed class MetaProgressionSaveData
    {
        public int SaveVersion = 1;
        public int MetaCurrency;
        public List<string> UnlockedContentIds = new();
        public List<string> DiscoveredContentIds = new();
        public List<string> SystemUnlockIds = new();
    }
}
