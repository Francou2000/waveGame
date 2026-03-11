using UnityEngine;

namespace WaveGame.Combat.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private PlayerStatsRuntime stats;
        [SerializeField] private float fallbackMoveSpeed = 6f;
        [SerializeField] private float gravity = -20f;

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
            var dt = Time.deltaTime;
            var input = PlayerInputReader.GetMoveInput();

            _lastMoveDirection = new Vector3(input.x, 0f, input.y);
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
    }
}
