using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace WaveGame.Combat.Player
{
    public static class PlayerInputReader
    {
        public static Vector2 GetMoveInput()
        {
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

#if ENABLE_INPUT_SYSTEM
            if (input.sqrMagnitude <= Mathf.Epsilon && Keyboard.current != null)
            {
                var x = 0f;
                var y = 0f;

                if (Keyboard.current.aKey.isPressed) x -= 1f;
                if (Keyboard.current.dKey.isPressed) x += 1f;
                if (Keyboard.current.sKey.isPressed) y -= 1f;
                if (Keyboard.current.wKey.isPressed) y += 1f;

                input = new Vector2(x, y);
            }
#endif

            return Vector2.ClampMagnitude(input, 1f);
        }

        public static bool IsPrimaryFireHeld()
        {
            if (Input.GetMouseButton(0))
            {
                return true;
            }

#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.isPressed;
#else
            return false;
#endif
        }
    }
}
