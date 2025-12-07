using SharpDX.XInput;
using System;

namespace MouseController
{
    public class GamepadReader
    {
        // 1. Dichiarazione degli Eventi
        // EventHandler<bool>: il booleano indica se il tasto è PREMUTO (true) o RILASCIATO (false)
        public event EventHandler<bool> ButtonA_StateChanged;

        public event EventHandler<bool> ButtonB_StateChanged;

        private const byte TriggerThreshold = 50;
        private bool _wasRTPressed = false;
        public event EventHandler<bool> RightTrigger_StateChanged; // true=DOWN, false=UP

        private bool _wasLTPressed = false;
        public event EventHandler<bool> LeftTrigger_StateChanged; // true=DOWN, false=UP


        private Controller _controller;
        private State _state;

        // 2. Variabili per la gestione dello stato precedente (Edge Detection/Anti-Spam)
        public bool _wasAButtonPressed = false;
        public bool _wasBButtonPressed = false;

        //MOVIMENTO MOUSE
        private const float Deadzone = 0.15f; // <--- AGGIUNGI LA DEADZONE
        //

        public GamepadReader()
        {
            _controller = new Controller(UserIndex.One);
        }

        public bool IsConnected => _controller.IsConnected;

        // Metodi ausiliari per la lettura dei pulsanti
        public bool IsButtonAPressed() => _state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
        public bool IsButtonBPressed() => _state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);

        /// <summary>
        /// Aggiorna lo stato del controller e solleva gli eventi in caso di cambio di stato.
        /// </summary>
        public void Update()
        {
            if (_controller.IsConnected)
            {
                _state = _controller.GetState();

                HandleButtonEvents();
            }
        }

        // Logica principale di Edge Detection e sollevamento eventi
        public void HandleButtonEvents()
        {
            // Tasto A (Sinistro)
            bool isAPressed = IsButtonAPressed();
            if (isAPressed && !_wasAButtonPressed)
            {
                // Stato è cambiato: RILASCIATO -> PREMUTO
                ButtonA_StateChanged?.Invoke(this, true); // Solleva l'evento: Tasto Giù
                _wasAButtonPressed = true;
            }
            else if (!isAPressed && _wasAButtonPressed)
            {
                // Stato è cambiato: PREMUTO -> RILASCIATO
                ButtonA_StateChanged?.Invoke(this, false); // Solleva l'evento: Tasto Su
                _wasAButtonPressed = false;
            }

            // Tasto B (Destro)
            bool isBPressed = IsButtonBPressed();
            if (isBPressed && !_wasBButtonPressed)
            {
                // Stato è cambiato: RILASCIATO -> PREMUTO
                ButtonB_StateChanged?.Invoke(this, true); // Solleva l'evento: Tasto Giù
                _wasBButtonPressed = true;
            }
            else if (!isBPressed && _wasBButtonPressed)
            {
                // Stato è cambiato: PREMUTO -> RILASCIATO
                ButtonB_StateChanged?.Invoke(this, false); // Solleva l'evento: Tasto Su
                _wasBButtonPressed = false;
            }

            // Tasto RT
            bool isRTPressed = _state.Gamepad.RightTrigger > TriggerThreshold;
            if (isRTPressed && !_wasRTPressed)
            {
                // Stato è cambiato: RILASCIATO -> PREMUTO
                RightTrigger_StateChanged?.Invoke(this, true); // Solleva l'evento: Grilletto Giù
                _wasRTPressed = true;
            }
            else if (!isRTPressed && _wasRTPressed)
            {
                // Stato è cambiato: PREMUTO -> RILASCIATO
                RightTrigger_StateChanged?.Invoke(this, false); // Solleva l'evento: Grilletto Su
                _wasRTPressed = false;
            }

            // Tasto LT
            bool isLTPressed = _state.Gamepad.LeftTrigger > TriggerThreshold;
            if (isLTPressed && !_wasLTPressed)
            {
                // Stato è cambiato: RILASCIATO -> PREMUTO
                LeftTrigger_StateChanged?.Invoke(this, true); // Solleva l'evento: Grilletto Giù
                _wasLTPressed = true;
            }
            else if (!isLTPressed && _wasLTPressed)
            {
                // Stato è cambiato: PREMUTO -> RILASCIATO
                LeftTrigger_StateChanged?.Invoke(this, false); // Solleva l'evento: Grilletto Su
                _wasLTPressed = false;
            }
        }

        //MOVIMENTO MOUSE

        /// <summary>
        /// Restituisce i valori normalizzati (-1.0 a 1.0) dello stick sinistro
        /// applicando la Deadzone per evitare drift.
        /// </summary>
        public float GetRightStickX()
        {
            if (!_controller.IsConnected) return 0f;

            // Lettura valore raw e normalizzazione
            float rawX = _state.Gamepad.RightThumbX / 32768f;

            // Applicazione della Deadzone (se il valore è troppo piccolo, ignoralo)
            if (Math.Abs(rawX) < Deadzone)
            {
                return 0f;
            }

            // Ricalcola la magnitudine (per dare piena spinta all'esterno della deadzone)
            float magnitude = Math.Abs(rawX);
            float normalizedMagnitude = (magnitude - Deadzone) / (1.0f - Deadzone);

            // Mantiene la direzione (segno)
            return Math.Sign(rawX) * normalizedMagnitude;
        }

        /// <summary>
        /// Restituisce il valore normalizzato (-1.0 a 1.0) dell'asse Y dello stick sinistro.
        /// </summary>
        public float GetRightStickY()
        {
            if (!_controller.IsConnected) return 0f;

            // Lettura valore raw e normalizzazione
            float rawY = _state.Gamepad.RightThumbY / 32768f;

            // Applicazione della Deadzone
            if (Math.Abs(rawY) < Deadzone)
            {
                return 0f;
            }

            // Ricalcola la magnitudine (per dare piena spinta all'esterno della deadzone)
            float magnitude = Math.Abs(rawY);
            float normalizedMagnitude = (magnitude - Deadzone) / (1.0f - Deadzone);

            // Mantiene la direzione (segno)
            return Math.Sign(rawY) * normalizedMagnitude;
        }

        public float GetLeftStickX()
        {
            if (!_controller.IsConnected) return 0f;

            // Lettura valore raw e normalizzazione
            float rawX = _state.Gamepad.LeftThumbX / 32768f;

            // Applicazione della Deadzone (se il valore è troppo piccolo, ignoralo)
            if (Math.Abs(rawX) < Deadzone)
            {
                return 0f;
            }

            // Ricalcola la magnitudine (per dare piena spinta all'esterno della deadzone)
            float magnitude = Math.Abs(rawX);
            float normalizedMagnitude = (magnitude - Deadzone) / (1.0f - Deadzone);

            // Mantiene la direzione (segno)
            return Math.Sign(rawX) * normalizedMagnitude;
        }

        public float GetLeftStickY()
        {
            if (!_controller.IsConnected) return 0f;

            // Lettura valore raw e normalizzazione
            float rawY = _state.Gamepad.LeftThumbY / 32768f;

            // Applicazione della Deadzone
            if (Math.Abs(rawY) < Deadzone)
            {
                return 0f;
            }

            // Ricalcola la magnitudine (per dare piena spinta all'esterno della deadzone)
            float magnitude = Math.Abs(rawY);
            float normalizedMagnitude = (magnitude - Deadzone) / (1.0f - Deadzone);

            // Mantiene la direzione (segno)
            return Math.Sign(rawY) * normalizedMagnitude;
        }
    }
}