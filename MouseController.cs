using System;
using System.Runtime.InteropServices;

namespace Pad2Mouse
{
    public class MouseController_Class
    {
        // 1. Structures and Definitions

        // Define the general structure of an input event
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // 2. Constants and Native API 

        const uint INPUT_MOUSE = 0;

        // MOVING MOUSE
        const uint MOUSEEVENTF_MOVE = 0x0001;

        // Flag for Left Click (RT Key)
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;

        // Flag for Right Click (LT Key)
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;

        // Flag for Mouse Wheel
        const uint MOUSEEVENTF_WHEEL = 0x0800;

        // Structure Dimension
        private static readonly int InputSize = Marshal.SizeOf(typeof(INPUT));

        // Native API for Windows to start the event
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        // 3. Control Methods

        // MOVING MOUSE
        public void Move(float dx, float dy)
        {
            int intDx = (int)Math.Round(dx);
            int intDy = (int)Math.Round(dy);

            if (intDx == 0 && intDy == 0) return;

            INPUT input = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT
                {
                    dx = intDx,
                    dy = intDy,
                    dwFlags = MOUSEEVENTF_MOVE
                }
            };

            SendInput(1, new INPUT[] { input }, InputSize);
        }

        // MOUSE LEFT CLICK
        public void LeftClick(bool down)
        {
            INPUT input = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT
                {
                    dwFlags = down ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP
                }
            };
            SendInput(1, new INPUT[] { input }, InputSize);
        }

        // MOUSE RIGHT CLICK
        public void RightClick(bool down)
        {
            INPUT input = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT
                {
                    dwFlags = down ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP
                }
            };
            SendInput(1, new INPUT[] { input }, InputSize);
        }

        public void Scroll(int amount)
        {
            INPUT input = new INPUT();
            input.type = INPUT_MOUSE;
            input.mi.dwFlags = MOUSEEVENTF_WHEEL;
            input.mi.mouseData = unchecked((uint)amount);

            SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}