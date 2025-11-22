using Mvk2.World.Biome;
using Mvk2.World.Gen.Layer;
using System.Diagnostics;
using System.Drawing;
using Vge.Games;
using Vge.World.Gen.Layer;

namespace Mvk2
{
    /// <summary>
    /// Отладочный класс для Малювек
    /// </summary>
    public class DebugMvk
    {

        /// <summary>
        /// Сделать скрин биомов определённого размера, при этом не надо ждать рендер чанков
        /// </summary>
        public static void ScreenFileBiomeArea(GameBase game)
        {
            //game.World.
            long seed = 4;
            // 0 - biome
            _FileArea(seed, 0, game);
            // 1 - height
            //_FileArea(seed, world, 1);
        }


        /// <summary>
        /// Сделать скрин биомов определённого размера, при этом не надо ждать рендер чанков
        /// </summary>
        private static void _FileArea(long seed, int id, GameBase game)
        {
            float timerFrequency = Stopwatch.Frequency / 1000;
            int width, height;
            width = height = 4096;
            //width = height = 8192;
            int zoom = 8;
            GenLayer[] genLayer = GenLayerIsland.BeginLayerBiome(seed);

            Bitmap bitmap = new Bitmap(width / zoom, height / zoom);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int x, z, key;

            int[] ar = genLayer[id].GetInts(width / -2, height / -2, width, height);
            long l = stopwatch.ElapsedTicks;
            game.Log.Log("GenBioms[{1}:{2}]: {0:0.00} ms",
                l / timerFrequency, width, height);
            EnumBiomeIsland biome;
            

            game.Log.Log("MansionInitLoading[{1}:{2}]: {0:0.00} ms",
               (stopwatch.ElapsedTicks - l) / timerFrequency, width, height);
            l = stopwatch.ElapsedTicks;

            for (x = 0; x < width / zoom; x++)
            {
                for (z = 0; z < height / zoom; z++)
                {
                    if (id == 0)
                    {
                        biome = (EnumBiomeIsland)ar[z * width * zoom + x * zoom];
                        if (biome != EnumBiomeIsland.Sea)
                        {
                            bitmap.SetPixel(x, z, _ConvertBiome(biome));
                        }
                    }
                    else
                    {
                        key = ar[z * width + x];
                        if (key != 0)
                        {
                            bitmap.SetPixel(x, z, _ConvertBiomeHeight(key));
                        }
                    }
                }
            }

            //for (x = 0; x < width; x++)
            //{
            //    for (z = 0; z < height; z++)
            //    {
            //        if (id == 0)
            //        {
            //            biome = (EnumBiome)ar[z * width + x];
            //            if (biome != EnumBiome.Sea)
            //            {
            //                bitmap.SetPixel(x, z, ConvertBiome(biome));
            //            }
            //        }
            //        else
            //        {
            //            key = ar[z * width + x];
            //            if (key != 0)
            //            {
            //                bitmap.SetPixel(x, z, ConvertBiomeHeight(key));
            //            }
            //        }
            //    }
            //}

            game.Log.Log("SetPixelGenBioms[{1}:{2}]: {0:0.00} ms",
                (stopwatch.ElapsedTicks - l) / timerFrequency, width, height);
            l = stopwatch.ElapsedTicks;

            /*
            MapGenObelisk genObelisk = new MapGenObelisk(world, null, seed);
            MapGenVillage genVillage = new MapGenVillage(world, null, seed);
            MapGenMansion genMansion = new MapGenMansion(world, null, seed);

            genObelisk.InitLoading(genLayer[0]);
            genMansion.InitLoading(genLayer[0]);

            width /= 16;
            height /= 16;
            int cob = 0;
            for (x = 0; x < width; x++)
            {
                for (z = 0; z < height; z++)
                {
                    if (genObelisk.Debug(genLayer[0], x - width / 2, z - height / 2))
                    {
                        cob++;
                        bitmap.SetPixel(x * 4 + 2, z * 4 + 2, Color.Red);
                        bitmap.SetPixel(x * 4 + 1, z * 4 + 2, Color.White);
                        strLog += "\r\n" + genObelisk.DebugMet(x - width / 2, z - height / 2) + ";";
                    }
                    if (genVillage.Debug(genLayer[0], x - width / 2, z - height / 2))
                    {
                        cob++;
                        bitmap.SetPixel(x * 4 + 2, z * 4 + 2, Color.Blue);
                        bitmap.SetPixel(x * 4 + 1, z * 4 + 2, Color.Yellow);
                    }
                    if (genMansion.Debug(genLayer[0], x - width / 2, z - height / 2))
                    {
                        cob++;
                        bitmap.SetPixel(x * 4 + 2, z * 4 + 2, Color.Violet);
                        bitmap.SetPixel(x * 4 + 1, z * 4 + 2, Color.Black);
                        bitmap.SetPixel(x * 4 + 0, z * 4 + 2, Color.White);
                    }
                }
            }
            
            game.Log.Log("GenObeliskList {0}", strLog);
            game.Log.Log("GenObelisk[{1}:{2}] {3}: {0:0.00} ms",
                (stopwatch.ElapsedTicks - l) / timerFrequency, width, height, cob);
            */
            bitmap.Save("biomeLayer" + (id == 0 ? "" : "H") + ".png", System.Drawing.Imaging.ImageFormat.Png);
        }

        private static Color _ConvertBiomeHeight(int index)
        {
            //if (index > 48) index = (index - 48) / 4 + 48;
            int rgb = 255 - index * 2;
            return Color.FromArgb(rgb, rgb, rgb);
        }

        private static Color _ConvertBiome(EnumBiomeIsland biome)
        {
            switch (biome)
            {
                case EnumBiomeIsland.Sea: return Color.Blue; // море
                case EnumBiomeIsland.River: return Color.MediumBlue; // река
                case EnumBiomeIsland.Plain: return Color.LightGreen; // Равнина
                case EnumBiomeIsland.Desert: return Color.Yellow; // Пустяня
                case EnumBiomeIsland.Beach: return Color.White; // Пляж
                case EnumBiomeIsland.MixedForest: return Color.Green; // Сешанный лес
                case EnumBiomeIsland.ConiferousForest: return Color.DarkGreen; // Хвойный лес
                case EnumBiomeIsland.BirchForest: return Color.ForestGreen; // Берёзовый лес
                case EnumBiomeIsland.Tropics: return Color.Orange; // Тропики
                case EnumBiomeIsland.Swamp: return Color.CadetBlue; // Болото
                case EnumBiomeIsland.Mountains: return Color.LightGray; // Горы
                case EnumBiomeIsland.MountainsDesert: return Color.Brown; // Горы в пустыне
            }
            return Color.Black;
        }
    }
}
