using UnityEngine;
using UnityEngine.InputSystem;

namespace WaveGame.Combat.Player
{
    public sealed class ThirdPersonCameraOrbit : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 targetOffset = new(0f, 1.6f, 0f);
        [SerializeField] private float distance = 6f;
        [SerializeField] private float followSmoothSpeed = 14f;

        [Header("Look")]
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private string lookActionName = "Look";
        [SerializeField] private float yawSpeed = 150f;
        [SerializeField] private float pitchSpeed = 120f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private bool invertY;

        [Header("Init")]
        [SerializeField] private bool alignYawToTargetOnStart = true;

        private InputAction _resolvedLookAction;
        private float _yaw;
        private float _pitch = 15f;

        private void Awake()
        {
            if (playerInput == null)
            {
                playerInput = FindFirstObjectByType<PlayerInput>();
            }

            ResolveLookAction();

            if (followTarget == null)
            {
                var anchor = FindFirstObjectByType<PlayerCombatAnchorProvider>();
                if (anchor != null)
                {
                    followTarget = anchor.transform;
                }
            }

            if (followTarget != null && alignYawToTargetOnStart)
            {
                _yaw = followTarget.eulerAngles.y;
            }
            else
            {
                _yaw = transform.eulerAngles.y;
            }

            _pitch = Mathf.Clamp(transform.eulerAngles.x > 180f ? transform.eulerAngles.x - 360f : transform.eulerAngles.x, minPitch, maxPitch);
        }

        private void OnEnable()
        {
            ResolveLookAction();
            if (lookAction != null)
            {
                _resolvedLookAction?.Enable();
            }
        }

        private void OnDisable()
        {
            if (lookAction != null)
            {
                _resolvedLookAction?.Disable();
            }
        }

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                return;
            }

            var lookDelta = ReadLookInput();
            _yaw += lookDelta.x * yawSpeed * Time.deltaTime;

            var pitchDelta = lookDelta.y * pitchSpeed * Time.deltaTime * (invertY ? 1f : -1f);
            _pitch = Mathf.Clamp(_pitch + pitchDelta, minPitch, maxPitch);

            var focus = followTarget.position + targetOffset;
            var desiredRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            var desiredPosition = focus - desiredRotation * Vector3.forward * Mathf.Max(0.5f, distance);

            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-followSmoothSpeed * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 1f - Mathf.Exp(-followSmoothSpeed * Time.deltaTime));
        }

        private Vector2 ReadLookInput()
        {
            if (_resolvedLookAction == null)
            {
                ResolveLookAction();
            }

            if (_resolvedLookAction != null)
            {
                return _resolvedLookAction.ReadValue<Vector2>();
            }

            var mouse = Mouse.current;
            return mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
        }

        private void ResolveLookAction()
        {
            _resolvedLookAction = lookAction != null ? lookAction.action : null;

            if (_resolvedLookAction == null && playerInput != null && playerInput.actions != null && !string.IsNullOrWhiteSpace(lookActionName))
            {
                _resolvedLookAction = playerInput.actions.FindAction(lookActionName, false);
            }
        }
    }
}
