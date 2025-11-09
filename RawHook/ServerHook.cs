using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RawHook
{
    public unsafe static class ServerHook
    {
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const int WH_KEYBOARD_LL = 13;

        public readonly struct KBDLLHOOKSTRUCT
        {
            public readonly uint vkCode;
            public readonly uint scanCode;
            public readonly uint flags;
            public readonly uint time;
            public readonly ulong dwExtraInfo;
        }
        public struct KeyBoardPackage
        {
            public KBDLLHOOKSTRUCT data;
            public int wParam;
            public int undefinded;
        }

        private static readonly void* _cb = (delegate* managed<int, void*, void*, void*>)&HookCallback;
        private static void* _hookID = null;

        public static uint New()
        {
            _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _cb, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
            return !(_hookID == null || _hookID == (void*)0L) ? 0 : GetLastError();
        }

        public static void Destroy()
        {
            if (_hookID == null)
                return;
            UnhookWindowsHookEx(_hookID);
            _hookID = null;
        }

        private static void* HookCallback(int nCode, void* wParam, void* lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (void*)WM_KEYDOWN || wParam == (void*)WM_SYSKEYDOWN || wParam == (void*)WM_KEYUP || wParam == (void*)WM_SYSKEYUP)
                {
                    KeyBoardPackage pack = new()
                    {
                        data = *(KBDLLHOOKSTRUCT*)lParam,
                        wParam = (int)wParam,
                        undefinded = 0xff
                    };
                    byte[] arr = new byte[32];
                    fixed (byte* ptr = arr)
                    {
                        *(KeyBoardPackage*)ptr = pack;
                    }

                    Server.Send32(arr);
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void* SetWindowsHookEx(int idHook, void* lpfn, void* hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(void* hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void* CallNextHookEx(void* hhk, int nCode, void* wParam, void* lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void* GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        private static extern uint GetLastError();
    }
}
