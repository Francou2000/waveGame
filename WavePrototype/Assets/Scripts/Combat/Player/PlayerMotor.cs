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
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string moveActionName = "Move";

        [Header("Movement Space")]
        [SerializeField] private bool moveRelativeToTransform = true;
        [SerializeField] private Transform movementReference;

        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string moveActionName = "Move";

        [Header("Movement Space")]
        [SerializeField] private bool moveRelativeToTransform = true;
        [SerializeField] private Transform movementReference;

        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string moveActionName = "Move";

        [Header("Movement Space")]
        [SerializeField] private bool moveRelativeToTransform = true;
        [SerializeField] private Transform movementReference;

        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string moveActionName = "Move";

        [Header("Movement Space")]
        [SerializeField] private bool moveRelativeToTransform = true;
        [SerializeField] private Transform movementReference;

        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string moveActionName = "Move";

        [Header("Movement Space")]
        [SerializeField] private bool moveRelativeToTransform = true;
        [SerializeField] private Transform movementReference;

        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string moveActionName = "Move";

        [Header("Movement Space")]
        [SerializeField] private bool moveRelativeToTransform = true;
        [SerializeField] private Transform movementReference;

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

            ResolveMovementReference();
            ResolveMoveAction();
        }

        private void OnEnable()
        {
            ResolveMovementReference();
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
            var dt = Time.deltaTime;
            var input = ReadMoveInput();

            _lastMoveDirection = ResolveMoveDirection(input);

            var speed = stats != null ? stats.MoveSpeed : fallbackMoveSpeed;
            var horizontal = _lastMoveDirection * speed;

            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -1f;
            }

            _verticalVelocity += gravity * dt;

            var velocity = horizontal + Vector3.up * _verticalVelocity;
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

        private Vector3 ResolveMoveDirection(Vector2 input)
        {
            var move = new Vector3(input.x, 0f, input.y);
            if (!moveRelativeToTransform || movementReference == null)
            {
                return move;
            }

            var forward = movementReference.forward;
            var right = movementReference.right;
            forward.y = 0f;
            right.y = 0f;

            if (forward.sqrMagnitude <= 0.0001f || right.sqrMagnitude <= 0.0001f)
            {
                return move;
            }

            forward.Normalize();
            right.Normalize();

            return right * input.x + forward * input.y;
        }

        private void ResolveMovementReference()
        {
            if (movementReference == null && Camera.main != null)
            {
                movementReference = Camera.main.transform;
            }
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
