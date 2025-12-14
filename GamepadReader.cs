using SharpDX.XInput;
using System;

namespace Pad2Mouse
{
    public class GamepadReader
    {
        // 1. EVENTS STATEMENT
        private const byte TriggerThreshold = 50;
        private float LeftDeadzone = 0.2f;
        private float RightDeadzone = 0.2f;

        private bool _wasRTPressed = false;
        public event EventHandler<bool> RightTrigger_StateChanged; // true = DOWN, false = UP

        private bool _wasLTPressed = false;
        public event EventHandler<bool> LeftTrigger_StateChanged; // true = DOWN, false = UP

        private Controller _controller;
        private State _state;

        // 2. VARIABLES FOR MANAGING THE PREVIOUS STATE (Edge Detection/Anti-Spam)
        public bool _wasRTButtonPressed = false;
        public bool _wasLTButtonPressed = false;

        // MOUSE MOVEMENT
        public void UpdateRightDeadzone(float NewDeadzone)
        {
            RightDeadzone = NewDeadzone;
        }

        public GamepadReader()
        {
            _controller = new Controller(UserIndex.One);
        }

        public bool IsConnected => _controller.IsConnected;

        // AUXILIARY METHODS FOR READING BUTTONS
        public bool IsButtonRTPressed() => _state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
        public bool IsButtonLTPressed() => _state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);

        // Upadates the status of the controller and raises events when there is a change of the status
        public void Update()
        {
            if (_controller.IsConnected)
            {
                _state = _controller.GetState();

                HandleButtonEvents();
            }
        }

        // Principal logic of Edge Detection and events raises
        public void HandleButtonEvents()
        {
            // Tasto RT (Left Click)
            bool isRTPressed = _state.Gamepad.RightTrigger > TriggerThreshold;
            if (isRTPressed && !_wasRTPressed)
            {
                // Status is changed: RELEASED -> PRESSED
                RightTrigger_StateChanged?.Invoke(this, true); // Solleva l'evento: Grilletto Giù
                _wasRTPressed = true;
            }
            else if (!isRTPressed && _wasRTPressed)
            {
                // Status is changed: PRESSED -> RELEASED
                RightTrigger_StateChanged?.Invoke(this, false); // Solleva l'evento: Grilletto Su
                _wasRTPressed = false;
            }

            // Tasto LT (Right Click)
            bool isLTPressed = _state.Gamepad.LeftTrigger > TriggerThreshold;
            if (isLTPressed && !_wasLTPressed)
            {
                // Status is changed: RELEASED -> PRESSED
                LeftTrigger_StateChanged?.Invoke(this, true); // Solleva l'evento: Grilletto Giù
                _wasLTPressed = true;
            }
            else if (!isLTPressed && _wasLTPressed)
            {
                // Status is changed: PRESSED -> RELEASED
                LeftTrigger_StateChanged?.Invoke(this, false); // Solleva l'evento: Grilletto Su
                _wasLTPressed = false;
            }
        }

        //MOUSE MOVEMENT

        // Return normalized values (-1.0 a 1.0) of the left stick
        // applying RightDeadzone to avoid drift and using separated
        // axis (X, Y) to improve precision of the movement so that
        // it applies the deadzone to each axis separately.

        // Return the normalized value (-1.0 a 1.0) of axis X of the right stick.
        public float GetRightStickX()
        {
            if (!_controller.IsConnected) return 0f;

            // Raw value reading and normalization
            float rawX = _state.Gamepad.RightThumbX / 32768f;

            // Application of RightDeadzone (if the value is too small, ignore it)
            if (Math.Abs(rawX) < RightDeadzone)
            {
                return 0f;
            }

            // Recalculate magnitude (to give full thrust outside the deadzone)
            float magnitude = Math.Abs(rawX);
            float normalizedMagnitude = (magnitude - RightDeadzone) / (1.0f - RightDeadzone);

            // Maintains direction (sign)
            return Math.Sign(rawX) * normalizedMagnitude;
        }

        // Return the normalized value (-1.0 a 1.0) of axis Y of the right stick.
        public float GetRightStickY()
        {
            if (!_controller.IsConnected) return 0f;

            // Raw value reading and normalization
            float rawY = _state.Gamepad.RightThumbY / 32768f;

            // Application of RightDeadzone
            if (Math.Abs(rawY) < RightDeadzone)
            {
                return 0f;
            }

            // Recalculate magnitude
            float magnitude = Math.Abs(rawY);
            float normalizedMagnitude = (magnitude - RightDeadzone) / (1.0f - RightDeadzone);

            // Maintains direction (sign)
            return Math.Sign(rawY) * normalizedMagnitude;
        }

        // Return the normalized value (-1.0 a 1.0) of axis Y of the left stick.
        public float GetLeftStickY()
        {
            if (!_controller.IsConnected) return 0f;

            // Raw value reading and normalization
            float rawY = _state.Gamepad.LeftThumbY / 32768f;

            // Application of RightDeadzone
            if (Math.Abs(rawY) < LeftDeadzone)
            {
                return 0f;
            }

            // Recalculate magnitude
            float magnitude = Math.Abs(rawY);
            float normalizedMagnitude = (magnitude - LeftDeadzone) / (1.0f - LeftDeadzone);

            // Maintains direction (sign)
            return Math.Sign(rawY) * normalizedMagnitude;
        }
    }
}