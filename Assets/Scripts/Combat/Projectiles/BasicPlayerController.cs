using UnityEngine;
using WaveGame.Combat.Player;

namespace WaveGame.Combat.Projectiles
{
    /// <summary>
    /// Controlador básico de prototipo:
    /// - Movimiento suave en plano XZ con WASD.
    /// - Rotación amortiguada hacia dirección de movimiento.
    /// - Disparo continuo mientras se mantiene Mouse0.
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
        [SerializeField] private float acceleration = 20f;
        [SerializeField] private float deceleration = 24f;
        [SerializeField] private float turnSmoothTime = 0.1f;

        [Header("Shooting")]
        [SerializeField] private float fireRate = 6f;

        private CharacterController _characterController;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private float _turnVelocity;
        private float _nextShotTime;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (TryGetComponent<WaveGame.Combat.Player.PlayerMotor>(out _))
            {
                Debug.LogWarning("BasicPlayerController disabled because PlayerMotor is present on the same GameObject.", this);
                enabled = false;
                return;
            }

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
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
            var input = PlayerInputReader.GetMoveInput();

            var moveDirection = GetMoveDirectionOnPlane(input);
            var targetHorizontalVelocity = moveDirection * moveSpeed;
            var blend = targetHorizontalVelocity.sqrMagnitude > 0.001f ? acceleration : deceleration;
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetHorizontalVelocity, blend * dt);

            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -1f;
            }

            _verticalVelocity += gravity * dt;
            var velocity = _horizontalVelocity + (Vector3.up * _verticalVelocity);

            _characterController.Move(velocity * dt);

            if (_horizontalVelocity.sqrMagnitude > 0.01f)
            {
                var targetYaw = Mathf.Atan2(_horizontalVelocity.x, _horizontalVelocity.z) * Mathf.Rad2Deg;
                var smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYaw, ref _turnVelocity, turnSmoothTime, Mathf.Infinity, dt);
                transform.rotation = Quaternion.Euler(0f, smoothYaw, 0f);
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

        private void HandleShooting()
        {
            if (weaponEmitter == null)
            {
                return;
            }

            if (!PlayerInputReader.IsPrimaryFireHeld())
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
