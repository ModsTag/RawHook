using System;

namespace RawHook
{
    internal class EntryPoint
    {
        static void Main(string[] args)
        {
            Server.Start();
            Server.MessageRepeat();
        }
    }
}
