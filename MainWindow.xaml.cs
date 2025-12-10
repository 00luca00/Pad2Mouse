using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace Pad2Mouse
{
    public partial class MainWindow : Window
    {

        // Instance variables for the controller classes
        private GamepadReader gamepadReader = new GamepadReader();
        private MouseController_Class mouseController = new MouseController_Class();

        // Tool to cancel the polling loop when the window is closed
        private CancellationTokenSource cts = new CancellationTokenSource();

        //MOUSE MOVEMENT
        private const float MouseSpeed = 15.0f; // Mouse speed (For future adjustments)

        // Preventing system sleep
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(uint esFlags);

        const uint ES_CONTINUOUS = 0x80000000;
        const uint ES_SYSTEM_REQUIRED = 0x00000001;

        public MainWindow()
        {
            InitializeComponent();

            // 1. Bind Gamepad events to mouse action methods
            SubscribeToGamepadEvents();

            // 2. Starts the polling loop on a background thread
            Task.Factory.StartNew(
                () => GamepadPollingLoop(cts.Token),
                cts.Token,
                TaskCreationOptions.LongRunning,    // Dedicated thread
                TaskScheduler.Default
            );

            // 3. Stop loop when window is closed
            this.Closing += (s, e) => cts.Cancel();
        }

        private void SubscribeToGamepadEvents()
        {
            // When the GamepadReader detects a state change for the RT button
            gamepadReader.RightTrigger_StateChanged += Mouse_LeftClick_Handler;

            // When the GamepadReader detects a state change for the lT button
            gamepadReader.LeftTrigger_StateChanged += Mouse_RightClick_Handler;
        }

        // - Event Handlers -
        private void Mouse_LeftClick_Handler(object sender, bool isDown)
        {
            // isDown is True when is pressed, False when is released (anti-spam guaranteed by GamepadReader)
            mouseController.LeftClick(isDown);
        }
        private void Mouse_RightClick_Handler(object sender, bool isDown)
        {
            mouseController.RightClick(isDown);
        }

        // - Asynchronous Polling Loop -

        private async Task GamepadPollingLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                gamepadReader.Update();

                // - CURSOR MOVEMENT LOGIC -
                if (gamepadReader.IsConnected)
                {
                    // METHODS FOR RIGHT ANALOG
                    // -------------------------------------------------
                    // Call the two separate methods for X and Y
                    float rx = gamepadReader.GetRightStickX();
                    float ry = gamepadReader.GetRightStickY();

                    // Calculate the speed
                    float dx = rx * MouseSpeed;

                    // The Y is reversed between the controller and the screen (Y must decrease to move up)
                    float dy = -ry * MouseSpeed;

                    if (dx != 0 || dy != 0)
                        mouseController.Move(dx, dy);
                }
                UpdateStatusText();

                await Task.Delay(2, token);
            }
        }
        private void UpdateStatusText()
        {
            // Dispatcher.Invoke is necessary because asynchronous loop cannot touch UI
            Dispatcher.InvokeAsync(() =>
            {
                if (gamepadReader.IsConnected)
                {
                    Status.Text = "Controller active";
                }
                else
                {
                    Status.Text = "Controller not detected";
                }
            });
        }
    }
}