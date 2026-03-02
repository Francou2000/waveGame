using UnityEngine;

namespace WaveGame.Combat.Projectiles
{
    /// <summary>
    /// Controlador básico de prototipo:
    /// - Movimiento en plano XZ con WASD.
    /// - Rotación hacia dirección de movimiento.
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
        [SerializeField] private float rotationLerpSpeed = 12f;

        [Header("Shooting")]
        [SerializeField] private float fireRate = 6f;

        private CharacterController _characterController;
        private float _verticalVelocity;
        private float _nextShotTime;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

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
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);

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

        private void HandleShooting()
        {
            if (weaponEmitter == null)
            {
                return;
            }

            if (!Input.GetMouseButton(0))
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
