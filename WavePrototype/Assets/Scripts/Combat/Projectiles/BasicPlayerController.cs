using UnityEngine;
using UnityEngine.InputSystem;
using WaveGame.Combat.Player;

namespace WaveGame.Combat.Projectiles
{
    /// <summary>
    /// Controlador básico de prototipo:
    /// - Movimiento en plano XZ con input action.
    /// - Rotación hacia dirección de movimiento.
    /// - Disparo continuo mientras se mantiene Attack.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class BasicPlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProjectileWeaponEmitter weaponEmitter;
        [SerializeField] private Transform cameraTransform;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float rotationLerpSpeed = 12f;

        [Header("Shooting")]
        [SerializeField] private float fireRate = 6f;

        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference attackAction;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string attackActionName = "Attack";

        [Header("Compatibility")]
        [SerializeField] private bool disableIfPlayerMotorPresent = true;

        private CharacterController _characterController;
        private float _verticalVelocity;
        private float _nextShotTime;
        private InputAction _resolvedMoveAction;
        private InputAction _resolvedAttackAction;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }

            ResolveActions();

            if (disableIfPlayerMotorPresent)
            {
                var playerMotor = GetComponent<PlayerMotor>();
                if (playerMotor != null && playerMotor.enabled)
                {
                    enabled = false;
                    return;
                }
            }
        }

        private void OnEnable()
        {
            ResolveActions();

            if (moveAction != null)
            {
                _resolvedMoveAction?.Enable();
            }

            if (attackAction != null)
            {
                _resolvedAttackAction?.Enable();
            }
        }

        private void OnDisable()
        {
            if (moveAction != null)
            {
                _resolvedMoveAction?.Disable();
            }

            if (attackAction != null)
            {
                _resolvedAttackAction?.Disable();
            }
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            HandleMovement(dt);
            HandleShooting();
        }

        private void HandleMovement(float dt)
        {
            var input = ReadMoveInput();

            var moveDirection = GetMoveDirectionOnPlane(input);
            var horizontalVelocity = moveDirection * moveSpeed;

            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -1f;
            }

            _verticalVelocity += gravity * dt;
            var velocity = horizontalVelocity + (Vector3.up * _verticalVelocity);

            _characterController.Move(velocity * dt);

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                var targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * dt);
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

            return (right * input.x + forward * input.y).normalized;
        }

        private Vector2 ReadMoveInput()
        {
            if (_resolvedMoveAction == null)
            {
                ResolveActions();
            }

            if (_resolvedMoveAction == null)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(_resolvedMoveAction.ReadValue<Vector2>(), 1f);
        }

        private bool IsAttackPressed()
        {
            if (_resolvedAttackAction == null)
            {
                ResolveActions();
            }

            return _resolvedAttackAction != null && _resolvedAttackAction.IsPressed();
        }

        private void ResolveActions()
        {
            _resolvedMoveAction = moveAction != null ? moveAction.action : null;
            _resolvedAttackAction = attackAction != null ? attackAction.action : null;

            if (playerInput == null || playerInput.actions == null)
            {
                return;
            }

            if (_resolvedMoveAction == null && !string.IsNullOrWhiteSpace(moveActionName))
            {
                _resolvedMoveAction = playerInput.actions.FindAction(moveActionName, false);
            }

            if (_resolvedAttackAction == null && !string.IsNullOrWhiteSpace(attackActionName))
            {
                _resolvedAttackAction = playerInput.actions.FindAction(attackActionName, false);
            }
        }

        private void HandleShooting()
        {
            if (weaponEmitter == null)
            {
                return;
            }

            if (!IsAttackPressed())
            {
                return;
            }

            if (Time.time < _nextShotTime)
            {
                return;
            }

            weaponEmitter.Fire();
            _nextShotTime = Time.time + (1f / Mathf.Max(0.01f, fireRate));
        }
    }
}
