using UnityEngine;

namespace WaveGame.Combat.Player
{
    public sealed class SmoothFollowCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Framing")]
        [SerializeField] private Vector3 followOffset = new(0f, 14f, -11f);
        [SerializeField] private Vector3 lookOffset = new(0f, 2f, 0f);

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.22f;
        [SerializeField] private float rotationSharpness = 9f;
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private float lookAheadSmoothTime = 0.3f;

        private Vector3 _positionVelocity;
        private Vector3 _lookAheadVelocity;
        private Vector3 _currentLookAhead;
        private PlayerMotor _cachedMotor;

        private void Awake()
        {
            if (target == null)
            {
                var player = FindFirstObjectByType<PlayerMotor>();
                if (player != null)
                {
                    target = player.transform;
                }
            }

            CacheMotor();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            CacheMotor();

            var desiredLookAhead = Vector3.zero;
            if (_cachedMotor != null)
            {
                var planarVelocity = _cachedMotor.HorizontalVelocity;
                planarVelocity.y = 0f;
                if (planarVelocity.sqrMagnitude > 0.01f)
                {
                    desiredLookAhead = planarVelocity.normalized * lookAheadDistance;
                }
            }

            _currentLookAhead = Vector3.SmoothDamp(_currentLookAhead, desiredLookAhead, ref _lookAheadVelocity, lookAheadSmoothTime);

            var anchor = target.position + _currentLookAhead;
            var desiredPosition = anchor + followOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _positionVelocity, positionSmoothTime);

            var lookPoint = anchor + lookOffset;
            var desiredRotation = Quaternion.LookRotation((lookPoint - transform.position).normalized, Vector3.up);
            var lerpFactor = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, lerpFactor);
        }

        private void CacheMotor()
        {
            if (_cachedMotor == null && target != null)
            {
                _cachedMotor = target.GetComponent<PlayerMotor>();
            }
        }
    }
}
