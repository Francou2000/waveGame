using UnityEngine;
using WaveGame.Combat.Interfaces;

namespace WaveGame.Combat.Damage
{
    public interface IDamageable : ITargetable
    {
        int TeamId { get; }
        Vector3 Position { get; }
        void ApplyDamage(DamageEvent damageEvent);
    }
}
