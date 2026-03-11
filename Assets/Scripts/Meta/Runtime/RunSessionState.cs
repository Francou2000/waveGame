using System.Collections.Generic;

namespace WaveGame.Meta.Runtime
{
    public sealed class RunSessionState
    {
        public readonly List<string> AvailableWeaponIds = new();
        public readonly List<string> AvailablePassiveIds = new();
        public readonly HashSet<string> BannedIds = new();
        public readonly HashSet<string> TakenThisRun = new();
        public int Seed;
    }
}
