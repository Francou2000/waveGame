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

        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string moveActionName = "Move";

        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string moveActionName = "Move";

        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string moveActionName = "Move";

        private CharacterController _controller;
        private float _verticalVelocity;
        private Vector3 _lastMoveDirection;
        private InputAction _resolvedMoveAction;

        public Vector3 LastMoveDirection => _lastMoveDirection;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (stats == null)
            {
                stats = GetComponent<PlayerStatsRuntime>();
            }

            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }

            ResolveMoveAction();
        }

        private void OnEnable()
        {
            ResolveMoveAction();

            if (moveAction != null)
            {
                _resolvedMoveAction?.Enable();
            }
        }

        private void OnDisable()
        {
            if (moveAction != null)
            {
                _resolvedMoveAction?.Disable();
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

        private Vector2 ReadMoveInput()
        {
            if (_resolvedMoveAction == null)
            {
                ResolveMoveAction();
            }

            if (_resolvedMoveAction == null)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(_resolvedMoveAction.ReadValue<Vector2>(), 1f);
        }

        private void ResolveMoveAction()
        {
            _resolvedMoveAction = moveAction != null ? moveAction.action : null;

            if (_resolvedMoveAction == null && playerInput != null && playerInput.actions != null && !string.IsNullOrWhiteSpace(moveActionName))
            {
                _resolvedMoveAction = playerInput.actions.FindAction(moveActionName, false);
            }
        }
    }
}
