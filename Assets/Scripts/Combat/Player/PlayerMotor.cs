using UnityEngine;

namespace WaveGame.Combat.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStatsRuntime stats;
        [SerializeField] private Transform cameraTransform;

        [Header("Movement")]
        [SerializeField] private float fallbackMoveSpeed = 6f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float deceleration = 22f;
        [SerializeField] private float airControlMultiplier = 0.45f;

        [Header("Facing")]
        [SerializeField] private float turnSmoothTime = 0.12f;

        private CharacterController _controller;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private float _turnVelocity;
        private Vector3 _lastMoveDirection = Vector3.forward;

        public Vector3 LastMoveDirection => _lastMoveDirection;
        public Vector3 HorizontalVelocity => _horizontalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (stats == null)
            {
                stats = GetComponent<PlayerStatsRuntime>();
            }

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            var input = PlayerInputReader.GetMoveInput();

            var desiredDirection = GetMoveDirectionOnPlane(input);
            var speed = stats != null ? stats.MoveSpeed : fallbackMoveSpeed;
            var desiredVelocity = desiredDirection * speed;

            var control = _controller.isGrounded ? 1f : airControlMultiplier;
            var blendRate = (desiredVelocity.sqrMagnitude > 0.01f ? acceleration : deceleration) * control;
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, desiredVelocity, blendRate * dt);

            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -1f;
            }

            _verticalVelocity += gravity * dt;

            var velocity = _horizontalVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * dt);

            if (_horizontalVelocity.sqrMagnitude > 0.01f)
            {
                _lastMoveDirection = _horizontalVelocity.normalized;
                RotateTowards(_lastMoveDirection, dt);
            }
        }

        private Vector3 GetMoveDirectionOnPlane(Vector2 input)
        {
            if (cameraTransform == null)
            {
                return new Vector3(input.x, 0f, input.y).normalized;
            }

            var forward = cameraTransform.forward;
            var right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            var direction = right * input.x + forward * input.y;
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        private void RotateTowards(Vector3 direction, float dt)
        {
            var targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            var currentYaw = transform.eulerAngles.y;
            var smoothYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref _turnVelocity, turnSmoothTime, Mathf.Infinity, dt);
            transform.rotation = Quaternion.Euler(0f, smoothYaw, 0f);
        }
    }
}
