using System;
using System.Diagnostics;
using Vge;
using WinGL;

namespace Mvk2
{
    class Program
    {
        //static void Main(string[] args)
        //{
        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();

        //    for (int i = 0; i < 20; i++)
        //    {
        //        stopwatch.Restart();
        //        Console.WriteLine("------");
        //        Test.TestPerformance();
        //        stopwatch.Stop();
        //        Console.WriteLine($"Блок: {stopwatch.ElapsedMilliseconds} ms");

        //    }
        //    Console.ReadLine();
        //}

        static void Main(string[] args)
        {
            Window.Run(new WindowMvk());
        }
    }
}
