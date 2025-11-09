using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RawHook
{
    public static class Server
    {
        // const(readonly)
        private static readonly byte[] header = [82, 97, 119, 72, 111, 111, 107, 80, 114, 111, 99, 101, 115, 115, 0, 0xff];

        // property
        public static bool Runnging => _running;
        // field
        private static bool _running;
        private static ServerForm _form;
        private static NamedPipeServerStream _pipe;

        // method
        public static void Start()
        {
            if (_running) return;
            _running = true;
            _form = new();
            _pipe = new NamedPipeServerStream("RawHookProcess", PipeDirection.InOut);

            Console.WriteLine("Waiting Client Connect...");
            _pipe.WaitForConnection();
            Console.WriteLine("Client Connected");
            _pipe.ReadMode = PipeTransmissionMode.Byte;

            Console.WriteLine("Check Header");
            byte[] h = new byte[16];
            _pipe.Read(h, 0, 16);
            bool res = true;
            for (int i = 0; i < 16; i++)
            {
                res = res || header[i] == h[i];
            }
            if (!res)
            {
                Console.WriteLine("Header FAIL");
                _pipe.Close();
                return;
            }
            Console.WriteLine("Header Success");

            Console.WriteLine("Start Hook");
            uint hook = ServerHook.New();
            if (hook > 0)
            {
                Console.WriteLine("Hook FAIL");
                Console.WriteLine("Last Error Code: " + hook);
                _pipe.Close();
                return;
            }
            Console.WriteLine("Hook Success");
        }
        public static void MessageRepeat()
        {
            Application.Run(new ApplicationContext(_form));
        }
        public static void Stop()
        {
            Console.WriteLine("Hook Stop");
            ServerHook.Destroy();
            _pipe.Disconnect();
            _pipe.Dispose();
            Application.Exit(new CancelEventArgs(false));
        }
        public static void Send(byte[] arr, int offset, int count)
        {
            TryWriteAsync(arr, offset, count);
        }
        public static void Send(byte[] arr)
        {
            TryWriteAsync(arr, 0, arr.Length);
        }
        public static void Send32(byte[] arr)
        {
            TryWriteAsync(arr, 0, 32);
        }
        private static async void TryWriteAsync(byte[] arr, int offset, int count)
        {
            try
            {
                await _pipe.WriteAsync(arr, offset, count);
            }
            catch
            {
                Stop();
            }
        }
    }
}
