using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SmartCapture;

public static class GlobalHotkey
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int pt_x;
        public int pt_y;
    }

    public static void Start(Action onAltSPressed, Action onAltKPressed)
    {
        Thread thread = new Thread(() =>
        {
            RegisterHotKey(IntPtr.Zero, 1, 0x0001, 0x53);
            RegisterHotKey(IntPtr.Zero, 2, 0x0001, 0x4B);

            MSG msg;
            while (GetMessage(out msg, IntPtr.Zero, 0, 0) > 0)
            {
                if (msg.message == 0x0312)
                {
                    int id = (int)msg.wParam;

                    if (id == 1) onAltSPressed?.Invoke();
                    else if (id == 2) onAltKPressed?.Invoke();
                }
            }
        })
        {
            IsBackground = true
        };

        thread.Start();
    }
}