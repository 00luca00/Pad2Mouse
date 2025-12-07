using System.Windows;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Windows.Threading; // Necessario per Dispatcher

namespace MouseController
{
    public partial class MainWindow : Window
    {
        private bool _isWPressed = false;
        private bool _isAPressed = false;
        private bool _isSPressed = false;
        private bool _isDPressed = false;

        private const float ActivationThreshold = 0.5f;
        private const float DeactivationThreshold = 0.2f;

        // Variabili di istanza per le classi controller
        private GamepadReader gamepadReader = new GamepadReader();
        private MouseController_Class mouseController = new MouseController_Class();

        // Strumento per annullare il loop di polling quando la finestra viene chiusa
        private CancellationTokenSource cts = new CancellationTokenSource();

        //MOVIMENTO MOUSE
        private const float MouseSpeed = 15.0f; // <--- VELOCITÀ DEL MOUSE (puoi modificarla)

        public MainWindow()
        {
            InitializeComponent();

            // 1. Collega gli eventi del Gamepad ai metodi di azione del mouse
            SubscribeToGamepadEvents();

            // 2. Avvia il loop di polling su un thread in background
            Task.Run(() => GamepadPollingLoop(cts.Token));

            // 3. Arresta il loop quando la finestra viene chiusa
            this.Closed += (s, e) => cts.Cancel();
        }

        private void SubscribeToGamepadEvents()
        {
            // Quando il GamepadReader rileva un cambio di stato per il Tasto RT
            gamepadReader.RightTrigger_StateChanged += Mouse_LeftClick_Handler;

            // Quando il GamepadReader rileva un cambio di stato per il Tasto LT
            gamepadReader.LeftTrigger_StateChanged += Mouse_RightClick_Handler;
        }

        // --- Gestori degli Eventi (Event Handlers) ---
        private void Mouse_LeftClick_Handler(object sender, bool isDown)
        {
            // isDown è True quando premuto, False quando rilasciato (anti-spam garantito dal GamepadReader)
            mouseController.LeftClick(isDown);
        }
        private void Mouse_RightClick_Handler(object sender, bool isDown)
        {
            mouseController.RightClick(isDown);
        }

        // --- Loop di Polling Asincrono ---

        private async Task GamepadPollingLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                gamepadReader.Update();

                // --- LOGICA DI MOVIMENTO DEL CURSORE ---
                if (gamepadReader.IsConnected)
                {
                    //METODI PER ANALOGICO DX
                    // -------------------------------------------------
                    // Chiama i due metodi separati per X e Y
                    float rx = gamepadReader.GetRightStickX();
                    float ry = gamepadReader.GetRightStickY();

                    // Calcola la velocità: lx * MouseSpeed
                    float dx = rx * MouseSpeed;

                    // La Y è invertita tra controller e schermo (per muoversi in alto Y deve diminuire)
                    float dy = -ry * MouseSpeed;

                    mouseController.Move(dx, dy);
                    // -------------------------------------------------
                    //METODI PER ANALOGICO SX
                    // -------------------------------------------------
                    float lx = gamepadReader.GetLeftStickX();
                    float ly = gamepadReader.GetLeftStickY();


                    // Gestione Asse Verticale (W/S)
                    bool shouldPressW = ly > ActivationThreshold;
                    bool shouldPressS = ly < -ActivationThreshold;

                    // --- GESTIONE TASTO W (In Alto) ---
                    if (!_isWPressed && ly > ActivationThreshold)
                    {
                        // PREMI W: Se non è premuto E superiamo la soglia alta
                        mouseController.PressW(true);
                        _isWPressed = true;
                    }
                    else if (_isWPressed && ly < DeactivationThreshold)
                    {
                        // RILASCIA W: Se è premuto E torniamo SOTTO la soglia bassa (vicino al centro)
                        mouseController.PressW(false);
                        _isWPressed = false;
                    }


                    // --- GESTIONE TASTO S (In Basso) ---
                    if (!_isSPressed && ly < -ActivationThreshold)
                    {
                        // PREMI S: Se non è premuto E scendiamo sotto la soglia negativa alta
                        mouseController.PressS(true);
                        _isSPressed = true;
                    }
                    /*else if (_isSPressed && ly > -DeactivationThreshold)
                    {
                        // RILASCIA S: Se è premuto E torniamo SOPRA la soglia negativa bassa (vicino al centro)
                        mouseController.PressS(false);
                        _isSPressed = false;
                    }*/


                    // --- GESTIONE TASTO D (A Destra) ---
                    if (!_isDPressed && lx > ActivationThreshold)
                    {
                        // PREMI D
                        mouseController.PressD(true);
                        _isDPressed = true;
                    }
                    else if (_isDPressed && lx < DeactivationThreshold)
                    {
                        // RILASCIA D
                        mouseController.PressD(false);
                        _isDPressed = false;
                    }

                    // --- GESTIONE TASTO A (A Sinistra) ---
                    if (!_isAPressed && lx < -ActivationThreshold)
                    {
                        // PREMI A
                        mouseController.PressA(true);
                        _isAPressed = true;
                    }
                    else if (_isAPressed && lx > -DeactivationThreshold)
                    {
                        // RILASCIA A
                        mouseController.PressA(false);
                        _isAPressed = false;
                    }


                    /*if (shouldPressW != _isWPressed)
                    {
                        mouseController.PressW(shouldPressW);
                        _isWPressed = shouldPressW;
                    }
                    if (shouldPressS != _isSPressed)
                    {
                        mouseController.PressS(shouldPressS);
                        _isSPressed = shouldPressS;
                    }

                    // Gestione Asse Orizzontale (A/D)
                    bool shouldPressD = lx > ActivationThreshol
                    bool shouldPressA = lx < -ActivationThreshold;

                    if (shouldPressD != _isDPressed)
                    {
                        mouseController.PressD(shouldPressD);wwwwwwwwwwwwwwwww
                        _isDPressed = shouldPressD;
                    }
                    if (shouldPressA != _isAPressed)
                    {
                        mouseController.PressA(shouldPressA);
                        _isAPressed = shouldPressA;
                    }*/

                }


                UpdateStatusText();

                await Task.Delay(10, token);
            }
        }

        /// <summary>
        /// Aggiorna l'elemento di testo 'Status' sulla UI in modo sicuro.
        /// </summary>
        private void UpdateStatusText()
        {
            // Dispatcher.Invoke è necessario perché il loop asincrono non può toccare la UI
            Dispatcher.Invoke(() =>
            {
                if (gamepadReader.IsConnected)
                {
                    // Assumendo che tu abbia un controllo 'Status' (es. TextBlock o Label) in XAML
                    Status.Text = "Controller attivo)";
                }
                else
                {
                    Status.Text = "Controller non rilevato";
                }
            });
        }
    }
}