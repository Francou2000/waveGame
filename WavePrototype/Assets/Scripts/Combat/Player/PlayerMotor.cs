using UnityEngine;
using UnityEngine.InputSystem;

namespace WaveGame.Combat.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private PlayerStatsRuntime stats;
        [SerializeField] private float fallbackMoveSpeed = 6f;
        [SerializeField] private float gravity = -20f;
        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;

        private CharacterController _controller;
        private float _verticalVelocity;
        private Vector3 _lastMoveDirection;

        public Vector3 LastMoveDirection => _lastMoveDirection;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (stats == null)
            {
                stats = GetComponent<PlayerStatsRuntime>();
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Vector2 input = ReadMoveInput();

            _lastMoveDirection = new Vector3(input.x, 0f, input.y);

            float speed = stats != null ? stats.MoveSpeed : fallbackMoveSpeed;
            Vector3 horizontal = _lastMoveDirection * speed;

            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -1f;
            }

            _verticalVelocity += gravity * dt;

            Vector3 velocity = horizontal + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * dt);
        }

        private void OnEnable()
        {
            if (moveAction != null)
            {
                moveAction.action?.Enable();
            }
        }

        private void OnDisable()
        {
            if (moveAction != null)
            {
                moveAction.action?.Disable();
            }
        }

        private Vector2 ReadMoveInput()
        {
            if (moveAction != null && moveAction.action != null)
            {
                return Vector2.ClampMagnitude(moveAction.action.ReadValue<Vector2>(), 1f);
            }

            return Vector2.zero;
        }
    }
}
