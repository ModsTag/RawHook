using System;
using System.Threading;

namespace RawHook_ClientDemo
{
    internal static class EntryPoint
    {
        internal static bool exit;
        private static void Main(string[] args)
        {
            Console.WriteLine("There is client demo");
            Console.WriteLine("you can pressed \"Esc\" to safe exit");
            Console.WriteLine("if you force quit, server will throw pipe broken exception");
            ClientHook.Start();
            while (!exit)
            {
                Thread.Sleep(2);
            }
            ClientHook.Stop();
        }
    }
}
