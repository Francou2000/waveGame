using UnityEngine;

namespace WaveGame.Combat.Player
{
    public sealed class PlayerCombatAnchorProvider : MonoBehaviour
    {
        [SerializeField] private Transform combatAnchor;

        public Vector3 Position => CombatAnchor.position;
        public Vector3 Forward => CombatAnchor.forward;
        public Transform CombatAnchor => combatAnchor != null ? combatAnchor : transform;

        private void Reset()
        {
            combatAnchor = transform;
        }
    }
}
