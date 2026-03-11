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

            Vector2 input = Vector2.zero;
            Keyboard keyboard = Keyboard.current;

            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
            }

            input = Vector2.ClampMagnitude(input, 1f);

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
    }
}