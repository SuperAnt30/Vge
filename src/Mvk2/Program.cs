using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vge.Entity.Animation;
using WinGL;
using WinGL.Util;

namespace Mvk2
{
    class Program
    {
        //static Dictionary<string, ModelAnimationClip> ddD = new Dictionary<string, ModelAnimationClip>();
        //static List<ModelAnimationClip> ddL = new List<ModelAnimationClip>();

        //static void TestInit()
        //{
        //    for (int i = 0; i < 32; i++)
        //    {
        //        ModelAnimationClip model = new ModelAnimationClip(i.ToString(), Vge.Entity.Model.ModelLoop.Loop, 0, 1, 1, new BoneAnimationChannel[] { });
        //        ddD.Add("123456789" + i.ToString(), model);
        //        ddL.Add(model);
        //    }
        //}

        //static double TestD(string s)
        //{
        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    ModelAnimationClip model;
        //    for (int J = 0; J < 10000; J++)
        //    { 
        //        if (ddD.ContainsKey(s)) 
        //            model = ddD[s];
        //    }
        //    stopwatch.Stop();
        //    Console.WriteLine($"D: {stopwatch.Elapsed.TotalMilliseconds}");
        //    return stopwatch.Elapsed.TotalMilliseconds;
        //}

        //static double TestL(int i)
        //{
        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    ModelAnimationClip model;
        //    for (int J = 0; J < 10000; J++)
        //    { 
        //        if (ddL.Count > i) model = ddL[i];
        //    }
        //    stopwatch.Stop();
        //    Console.WriteLine($"L: {stopwatch.Elapsed.TotalMilliseconds}");
        //    return stopwatch.Elapsed.TotalMilliseconds;
        //}

        static void Main(string[] args)
        {

            //Glm.Initialized();
            //Vector3 v = new Vector3(1, 0, 0);
            //Vector3 v2 = new Vector3(1, 0, 0);
            //for (int i = -180; i < 360; i += 5)
            //{
            //    v2 = Glm.Rotate(new Vector3(1, 0, 0), Glm.Radians(i), new Vector3(0, 1, 0));
            //    Console.WriteLine(i + " = " + Glm.Dot(v, v2));
            //}
            //Console.Read();



            Window.Run(new WindowMvk());

            //TestInit();
            //double t = 0;
            //for (int i = 0; i < 5; i++)
            //{
            //    t += TestD("1234567895");
            //}
            //Console.WriteLine($"5 ==: {t}");
            //t = 0;
            //for (int i = 0; i < 5; i++)
            //{
            //    t += TestD("12345678920"); 
            //}
            //Console.WriteLine($"20 ==: {t}");
            //t = 0;
            //for (int i = 0; i < 5; i++)
            //{
            //    t += TestL(5);
            //}
            //Console.WriteLine($" 5==: {t}");
            //t = 0;
            //for (int i = 0; i < 5; i++)
            //{
            //    t += TestL(20);
            //}
            //Console.WriteLine($" 20==: {t}");
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
