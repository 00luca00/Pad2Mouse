using System;
using System.Runtime.InteropServices;

namespace MouseController
{
    public class MouseController_Class
    {
        // --- 1. Strutture e Definizioni Win32 ---

        // Definisce la struttura generale di un evento di input
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

        // Struttura per i dati della Tastiera
        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // Struttura per i dati Hardware generici
        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        // --- 2. La Union (INPUT_UNION) ---

        // Questa struttura sovrappone i campi del Mouse, Tastiera e Hardware.
        [StructLayout(LayoutKind.Explicit)]
        struct INPUT_UNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        // --- 3. La Struttura INPUT Generale (quella usata in SendInput) ---

        // La struttura finale usata dall'API Win32
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            // 0 = Mouse, 1 = Tastiera, 2 = Hardware
            public uint type;

            // Contiene i dati del Mouse, Tastiera o Hardware (a seconda di 'type')
            public INPUT_UNION U;
        }

        // Costanti Aggiuntive
        const uint INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYDOWN = 0x0000;
        const uint KEYEVENTF_KEYUP = 0x0002;

        // Codici Virtuali (VK) per WASD
        const ushort VK_W = 0x57; // 'W'
        const ushort VK_A = 0x41; // 'A'
        const ushort VK_S = 0x53; // 'S'
        const ushort VK_D = 0x44; // 'D'


        // --- 2. Costanti e API Native ---

        const uint INPUT_MOUSE = 0;

        //MUOVERE MOUSE
        const uint MOUSEEVENTF_MOVE = 0x0001; // <--- AGGIUNGI QUESTA

        // Flag per il Tasto Sinistro (Associato al Tasto A)
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;

        // Flag per il Tasto Destro (Associato al Tasto B)
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;

        // Dimensione della struttura pre-calcolata
        private static readonly int InputSize = Marshal.SizeOf(typeof(INPUT));

        // Importazione dell'API nativa di Windows per iniettare l'input
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        // --- 3. Metodi di Controllo ---

        //MUOVERE MOUSE
        public void Move(float dx, float dy)
        {
            int intDx = (int)Math.Round(dx);
            int intDy = (int)Math.Round(dy);

            if (intDx == 0 && intDy == 0) return;

            INPUT input = new INPUT
            {
                type = INPUT_MOUSE,
                U = new INPUT_UNION // <--- Accede al campo Union
                {
                    mi = new MOUSEINPUT // <--- Accede al campo Mouse all'interno della Union
                    {
                        dx = intDx,
                        dy = intDy,
                        dwFlags = MOUSEEVENTF_MOVE
                    }
                }
            };

            SendInput(1, new INPUT[] { input }, InputSize);
        }

        // LEFT CLICK DEL MOUSE
        public void LeftClick(bool down)
        {
            INPUT input = new INPUT
            {
                type = INPUT_MOUSE,
                U = new INPUT_UNION // <--- Accede al campo Union
                {
                    mi = new MOUSEINPUT // <--- Accede al campo Mouse all'interno della Union
                    {
                        dwFlags = down ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP
                    }
                }
            };
            SendInput(1, new INPUT[] { input }, InputSize);
        }

        // RIGHT CLICK DEL MOUSE
        public void RightClick(bool down)
        {
            INPUT input = new INPUT
            {
                type = INPUT_MOUSE,
                U = new INPUT_UNION
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = down ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP
                    }
                }
            };
            SendInput(1, new INPUT[] { input }, InputSize);
        }

        // TASTIERA
        private void SimulateKey(ushort keyCode, bool down)
        {
            // Creiamo un'istanza pulita per evitare dati residui
            INPUT input = new INPUT();

            input.type = INPUT_KEYBOARD;

            // Inizializziamo in modo esplicito la struttura KEYBDINPUT
            input.U.ki = new KEYBDINPUT
            {
                wVk = keyCode,
                wScan = 0, // Fondamentale: usa 0 quando si usa wVk
                dwFlags = down ? KEYEVENTF_KEYDOWN : KEYEVENTF_KEYUP,
                time = 0,
                dwExtraInfo = IntPtr.Zero
            };

            // La pressione continua è gestita da SendInput che blocca il tasto
            // finché non riceve l'evento KEYEVENTF_KEYUP.
            SendInput(1, new INPUT[] { input }, InputSize);
        }

        // Metodi WASD pubblici (chiameranno SimulateKey)
        public void PressW(bool down) => SimulateKey(VK_W, down);
        public void PressA(bool down) => SimulateKey(VK_A, down);
        public void PressS(bool down) => SimulateKey(VK_S, down);
        public void PressD(bool down) => SimulateKey(VK_D, down);
    }
}