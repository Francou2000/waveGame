using UnityEngine;

namespace WaveGame.Combat.Player
{
    public sealed class PlayerAim : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private bool faceMovementDirection = true;
        [SerializeField] private float rotationLerpSpeed = 12f;

        private void Awake()
        {
            if (motor == null)
            {
                motor = GetComponent<PlayerMotor>();
            }
        }

        private void Update()
        {
            if (!faceMovementDirection || motor == null)
            {
                return;
            }

            var dir = motor.LastMoveDirection;
            if (dir.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
        }
    }
}
