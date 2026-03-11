using UnityEngine;

namespace WaveGame.Combat.Interfaces
{
    public interface ITargetable
    {
        int EntityId { get; }
        Vector3 GetAimPoint();
        bool IsAlive { get; }
    }
}
