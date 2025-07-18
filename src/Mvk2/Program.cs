using WinGL;

namespace Mvk2
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //Glm.Initialized();
            //Vector3 v = new Vector3(1, 0, 0);
            //Vector3 v2 = new Vector3(1, 0, 0);
            //for (int i = -180; i < 360; i+=5)
            //{
            //    v2 = Glm.Rotate(new Vector3(1, 0, 0), Glm.Radians(i), new Vector3(0, 1, 0));
            //    Console.WriteLine(i + " = " + Glm.Dot(v, v2));
            //}
            //Console.Read();
            
            

            Window.Run(new WindowMvk());

            //double t = 0;
            //for (int i = 0; i < 25; i++)
            //{
            //    t += Test();
            //}
            //Console.WriteLine($" ==: {t}");
            //Console.Read();
        }

        //private static double Test()
        //{
        //    Stopwatch stopwatch = new Stopwatch();
            
        //    WritePacket writePacket = new WritePacket();
        //    ReadPacket readPacket = new ReadPacket();
        //    PacketC00Ping packet;
        //    byte[] array = new byte[1200];
        //    stopwatch.Start();
        //    for (int i = 0; i < 45000; i++)
        //    {
        //        packet = new PacketC00Ping(5, 6, 12, 42.5f, 5.5f, 5.6f, false, array);
        //        writePacket.Trancive(packet);
        //        readPacket.Receive(writePacket.ToArray(), packet);
        //    }
        //    stopwatch.Stop();
        //    Console.WriteLine($"Weaver: {stopwatch.Elapsed.TotalMilliseconds}");
        //    return stopwatch.Elapsed.TotalMilliseconds;
        //}
    }
}
