using UnityEngine;

namespace WaveGame.Combat.Player
{
    public sealed class PlayerAim : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private bool faceMovementDirection = true;
        [SerializeField] private float rotationLerpSpeed = 12f;

        [Header("Target")]
        [SerializeField] private Transform rotationTarget;

        private void Awake()
        {
            if (motor == null)
            {
                motor = GetComponent<PlayerMotor>();
            }
        }

        private void Update()
        {
            if (!faceMovementDirection || motor == null || rotationTarget == null)
            {
                return;
            }

            var dir = motor.LastMoveDirection;
            dir.y = 0f;
            if (dir.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            rotationTarget.rotation = Quaternion.Slerp(rotationTarget.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
        }
    }
}
