using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;

namespace RawHook_ClientDemo
{
    public static class ClientHook
    {
        public readonly struct KBDLLHOOKSTRUCT
        {
            public readonly uint vkCode;
            public readonly uint scanCode;
            public readonly uint flags;
            public readonly uint time;
            public readonly ulong dwExtraInfo;
        }
        public readonly struct KeyBoardPackage
        {
            public readonly KBDLLHOOKSTRUCT data;
            public readonly int wParam;
            public readonly int undefinded;
        }
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        public static bool Running => _running;
        internal static readonly long[] RealInputTime = new long[256];
        private static bool _running;
        private static Process _proc;
        private static NamedPipeClientStream _pipe;
        private static Thread _pipeThread;

        internal static void Start()
        {
            if (Running) return;
            // create server process
            _proc = new Process();
#if DEBUG
            _proc.StartInfo.FileName = "RawHook.exe";
            _proc.StartInfo.CreateNoWindow = false;
            _proc.StartInfo.UseShellExecute = true;
#else
            _proc.StartInfo.FileName = "RawHook.exe";
            _proc.StartInfo.CreateNoWindow = true;
            _proc.StartInfo.UseShellExecute = false;
#endif
            _proc.Start();
            // pipe connect
            _pipe = new NamedPipeClientStream(".", "RawHookProcess", PipeDirection.InOut);
            _pipe.Connect(3000);
            // magic number to match and test
            _pipe.Write(new byte[16] { 82, 97, 119, 72, 111, 111, 107, 80, 114, 111, 99, 101, 115, 115, 0, 0xff }, 0, 16);
            // pipe thread, use to progesss
            _pipeThread = new Thread(new ThreadStart(NewThread))
            {
                IsBackground = true
            };
            _running = true;
            _pipeThread.Start();
        }

        internal static void Stop()
        {
            if (!_running) return;
            _pipe.Dispose();
            _running = false;
            while (_pipeThread.ThreadState == System.Threading.ThreadState.Running) ;
        }

        internal unsafe static void NewThread()
        {
            while (_running)
            {
                byte[] arr = new byte[32];
                _pipe.Read(arr, 0, 32);
                KeyBoardPackage pack;
                fixed (byte* ptr = arr)
                {
                    pack = *(KeyBoardPackage*)ptr;
                }
                if (pack.undefinded != 0xff)
                {
                    /*
                     * Error Program
                     */
                    continue;
                }
                if (pack.wParam == WM_KEYDOWN || pack.wParam == WM_SYSKEYDOWN)
                {
                    /*
                     * Pressed Program
                     * e.g: keys[pack.data.vkCode] = true;
                     */
                }
                else if (pack.wParam == WM_KEYUP || pack.wParam == WM_SYSKEYUP)
                {
                    /*
                     * Released Program
                     * e.g: keys[pack.data.vkCode] = false;
                     */
                }
                /*
                 * program by all key input
                 */
                if (pack.data.vkCode == 0x1B) // escpace
                    EntryPoint.exit = true;

                Console.WriteLine($"Get Key Input: wParam={pack.wParam}, vkCode={pack.data.vkCode}, time={pack.data.time}");
                RealInputTime[pack.data.vkCode] = pack.data.time;
            }
        }
    }
}
