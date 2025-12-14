using MouseController;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Pad2Mouse
{
    public partial class MainWindow : Window
    {

        // Instance variables for the controller classes
        private GamepadReader gamepadReader = new GamepadReader();
        private MouseController_Class mouseController = new MouseController_Class();

        // Tool to cancel the polling loop when the window is closed
        private CancellationTokenSource cts = new CancellationTokenSource();

        // MOUSE SPEED
        private float CurrentMouseSpeed = 10.0f;
        private float PendingMouseSpeed = 10.0f;

        // MOUSE DEADZONE
        private float PendingMouseDeadzone = 0.2f;

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

                if (gamepadReader.IsConnected)
                {
                    // - CURSOR MOVEMENT LOGIC -

                    // METHODS FOR RIGHT ANALOG
                    // -------------------------------------------------
                    // Call the two separate methods for X and Y
                    float rx = gamepadReader.GetRightStickX();
                    float ry = gamepadReader.GetRightStickY();

                    // Calculate the speed
                    float dx = rx * CurrentMouseSpeed;

                    // The Y is reversed between the controller and the screen (Y must decrease to move up)
                    float dy = -ry * CurrentMouseSpeed;

                    if (dx != 0 || dy != 0)
                        mouseController.Move(dx, dy);

                    // - SCROLLING LOGIC -
                    float ly = gamepadReader.GetLeftStickY();

                    int scrollAmount = (int)(ly * 40);
                    mouseController.Scroll(scrollAmount);
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
                    Status.Text = "Pad2Mouse";
                    SettingsPanel.Visibility = Visibility.Visible;
                    HelpButton.Visibility = Visibility.Visible;
                }
                else
                {
                    Status.Text = "Controller not detected";
                    SettingsPanel.Visibility = Visibility.Collapsed;
                    HelpButton.Visibility = Visibility.Collapsed;
                }
            });
        }
        // METHOD TO HELP BUTTON
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Use the right analog stick to move the mouse cursor.\n" +
                "Use the left analog stick to scroll up and down.\n\n" +
                "Press the RT button to perform a left click.\n" +
                "Press the LT button to perform a right click.\n\n" +
                "Adjust the sensitivity and deadzone sliders to customize the mouse behavior.\n\n",
                "Help", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        // METHOD TO USE SENSITIVITY SLIDER
        private void SensitivitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PendingMouseSpeed = (float)e.NewValue;
            if (SensitivityValue != null)
                SensitivityValue.Text = PendingMouseSpeed.ToString("R");
            if (ConfirmButton != null)
                ConfirmButton.Visibility = Visibility.Visible;
        }

        // METHOD TO USE DEADZONE SLIDER
        private void DeadzoneSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PendingMouseDeadzone = (float)(e.NewValue / 100.0);
            if (DeadzoneValue != null)
                DeadzoneValue.Text = PendingMouseDeadzone.ToString("R");
            if (ConfirmButton != null)
                ConfirmButton.Visibility = Visibility.Visible;
        }

        // METHOD TO APPLY SENSITIVITY TO CONTROLLER
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySensitivity();
            ApplyDeadzone();
            ConfirmButton.Visibility = Visibility.Collapsed;
        }

        // METHOD TO CHANGE CURRENT SENSITIVITY
        private void ApplySensitivity()
        {
            CurrentMouseSpeed = PendingMouseSpeed;
        }

        // METHOD TO CHANGE CURRENT DEADZONE FOR MOUSE CURSOR
        private void ApplyDeadzone()
        {
            gamepadReader.UpdateRightDeadzone(PendingMouseDeadzone);
        }
    }
}