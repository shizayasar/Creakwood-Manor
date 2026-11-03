using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace CreakwoodManor.Controls
{
    public enum KeyboardSide
    {
        Left,   // Human: WASD + E + R
        Right   // Ghost: arrow keys + Right Shift + Enter
    }

    /// <summary>
    /// Reads one half of a shared keyboard and exposes it as IPlayerInput.
    /// Put one on the Human (Left) and one on the Ghost (Right).
    /// Runs before other scripts so characters always see this frame's input.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class KeyboardPlayerInput : MonoBehaviour, IPlayerInput
    {
        [SerializeField] private KeyboardSide side = KeyboardSide.Left;

        public Vector2 Move { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool PossessPressed { get; private set; }
        public bool AbilityPressed { get; private set; }
        public bool RestartPressed { get; private set; }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                ClearInput();
                return;
            }

            if (side == KeyboardSide.Left)
            {
                ReadLeftSide(keyboard);
            }
            else
            {
                ReadRightSide(keyboard);
            }
        }

        private void ReadLeftSide(Keyboard keyboard)
        {
            Move = new Vector2(
                Axis(keyboard.aKey, keyboard.dKey),
                Axis(keyboard.sKey, keyboard.wKey));

            JumpPressed = keyboard.wKey.wasPressedThisFrame;
            JumpHeld = keyboard.wKey.isPressed;
            InteractPressed = keyboard.eKey.wasPressedThisFrame;
            RestartPressed = keyboard.rKey.wasPressedThisFrame;

            // The human can't possess things.
            PossessPressed = false;
            AbilityPressed = false;
        }

        private void ReadRightSide(Keyboard keyboard)
        {
            Move = new Vector2(
                Axis(keyboard.leftArrowKey, keyboard.rightArrowKey),
                Axis(keyboard.downArrowKey, keyboard.upArrowKey));

            PossessPressed = keyboard.rightShiftKey.wasPressedThisFrame;
            AbilityPressed = keyboard.enterKey.wasPressedThisFrame
                             || keyboard.numpadEnterKey.wasPressedThisFrame;

            // The ghost floats instead of jumping, and R lives on the left side
            // so one key press never restarts the room twice.
            JumpPressed = false;
            JumpHeld = false;
            InteractPressed = false;
            RestartPressed = false;
        }

        private void ClearInput()
        {
            Move = Vector2.zero;
            JumpPressed = JumpHeld = InteractPressed = false;
            PossessPressed = AbilityPressed = RestartPressed = false;
        }

        /// <summary>Turns two keys into -1, 0 or 1.</summary>
        private static float Axis(KeyControl negative, KeyControl positive)
        {
            float value = 0f;
            if (positive.isPressed) value += 1f;
            if (negative.isPressed) value -= 1f;
            return value;
        }
    }
}
