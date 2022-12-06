using System;
using System.Collections.Concurrent;


namespace laba
{
    class Program
    {
        public static void Main()
        {
            Console.WriteLine("Tests passed: " + (Tests.RunPushTest() & Tests.RunPopTests()));
        }
    }
}