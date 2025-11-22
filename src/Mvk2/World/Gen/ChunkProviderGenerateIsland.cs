using Mvk2.World.Gen.Layer;
using Vge.Games;
using Vge.Util;
using Vge.World;
using Vge.World.Chunk;
using Vge.World.Gen;
using Vge.World.Gen.Layer;

namespace Mvk2.World.Gen
{
    /// <summary>
    /// Объект генерации чанка для мира, генерация Остров
    /// </summary>
    public class ChunkProviderGenerateIsland : IChunkProviderGenerate
    {
        /// <summary>
        /// Количество секций в чанке. Максимально 32
        /// </summary>
        public readonly byte NumberChunkSections;

        /// <summary>
        /// Вспомогательный рандом
        /// </summary>
        public Rand Rnd { get; private set; }
        /// <summary>
        /// Зерно генерации случайных чисел игры
        /// </summary>
        public long Seed { get; private set; }

        /// <summary>
        /// Шум нижнего слоя
        /// </summary>
        public NoiseGeneratorPerlin NoiseDown { get; private set; }

        /// <summary>
        /// Чанк для заполнения данных
        /// </summary>
        private readonly ChunkPrimerIsland _chunkPrimer;

        /// <summary>
        /// Объект генерации слоёв биомов
        /// </summary>
        private readonly GenLayer _genLayerBiome;
        /// <summary>
        /// Объект генерации слоёв высот от биомов
        /// </summary>
        private readonly GenLayer _genLayerHeight;

        public ChunkProviderGenerateIsland(byte numberChunkSections)
        {
            NumberChunkSections = numberChunkSections;
            _chunkPrimer = new ChunkPrimerIsland(numberChunkSections);

            GenLayer[] gens = GenLayerIsland.BeginLayerBiome(Seed);
            _genLayerBiome = gens[0];
            _genLayerHeight = gens[1];
        }

        public void InitLoading(GameServer server, WorldServer worldServer)
        {
            Seed = server.Settings.Seed;
            Rnd = new Rand(Seed);

            NoiseDown = new NoiseGeneratorPerlin(new Rand(Seed), 1);
        }

        private readonly float[] downNoise = new float[256];

        public void GenerateChunk(ChunkBase chunk)
        {
            _chunkPrimer.Clear();

            int xbc = chunk.CurrentChunkX << 4;
            int zbc = chunk.CurrentChunkY << 4;

            float scale = 1.0f;

            NoiseDown.GenerateNoise2d(downNoise, xbc, zbc, 16, 16, scale, scale);

            int x, y, z, idx;
            int count = 0;

            // Низ бедрок
            for (x = 0; x < 16; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    float n = downNoise[count];
                    count++;
                    _chunkPrimer.SetBlockState(x, 0, z, 2);
                    _chunkPrimer.SetBlockState(x, 1, z, (ushort)(n < .1 ? 2 : 3));
                    _chunkPrimer.SetBlockState(x, 2, z, (ushort)(n < -.1 ? 2 : 3));
                }
            }

            int[] arHeight = _genLayerHeight.GetInts(xbc, zbc, 16, 16);
            int[] arBiome = _genLayerBiome.GetInts(xbc, zbc, 16, 16);

            int h, b;
            for (x = 0; x < 16; x++)
            {
                for (z = 0; z < 16; z++)
                {
                    idx = z << 4 | x;
                    h = arHeight[idx];
                    if (h > 2)
                    {
                        for (y = 3; y < h; y++)
                        {
                            _chunkPrimer.SetBlockState(x, y, z, 4);
                        }
                        b = arBiome[idx];
                        _chunkPrimer.SetBlockState(x, h, z, (ushort)(b + 1));
                    }
                    //_chunkPrimer.SetBlockState(x, 3, z, (ushort)(h + 1));
                }
            }

            //int h = chunk.NumberSections == 8 ? 47 : 95;
            //// Временно льём тест

            //ushort Stone = BlocksRegMvk.Stone.IndexBlock;
            //ushort Cobblestone = BlocksRegMvk.Cobblestone.IndexBlock;
            //ushort Limestone = BlocksRegMvk.Limestone.IndexBlock;
            //ushort Granite = BlocksRegMvk.Granite.IndexBlock;
            //ushort Glass = BlocksRegMvk.Glass.IndexBlock;
            //ushort GlassRed = BlocksRegMvk.GlassRed.IndexBlock;
            //ushort GlassBlue = BlocksRegMvk.GlassBlue.IndexBlock;
            //ushort GlassPurple = BlocksRegMvk.GlassPurple.IndexBlock;
            //ushort GlassGreen = BlocksRegMvk.GlassGreen.IndexBlock;
            //ushort FlowerClover = BlocksRegMvk.FlowerClover.IndexBlock;
            //ushort Water = BlocksRegMvk.Water.IndexBlock;
            //ushort Lava = BlocksRegMvk.Lava.IndexBlock;
            ////ushort Brol = BlocksRegMvk.Brol.IndexBlock;

            //for (x = 0; x < 16; x++)
            //{
            //    for (z = 0; z < 16; z++)
            //    {
            //        for (y = 0; y < h; y++)
            //        {
            //            _chunkPrimer.SetBlockState(x, y, z, Stone);
            //        }
            //    }
            //}

            ChunkStorage chunkStorage;
           
            int yc, ycb, y0;
            ushort id;
            int index, indexY, indexYZ;
            for (yc = 0; yc < NumberChunkSections; yc++)
            {
                ycb = yc << 4;
                chunkStorage = chunk.StorageArrays[yc];
                for (y = 0; y < 16; y++)
                {
                    y0 = ycb | y;
                    if (y0 <= chunk.Settings.NumberMaxBlock)
                    {
                        indexY = y0 << 8;
                        for (z = 0; z < 16; z++) 
                        {
                            indexYZ = indexY | z << 4;
                            for (x = 0; x < 16; x++)
                            {
                                index = indexYZ | x;
                                id = _chunkPrimer.Id[index];
                                if (id != 0)
                                {
                                    chunkStorage.SetData(y << 8 | z << 4 | x, id, _chunkPrimer.Met[index]);
                                }
                            }
                        }
                    }
                }
            }

            //World.Filer.EndSectionLog(); // 0.3 мс
            //World.Filer.StartSection("GHM " + CurrentChunkX + "," + CurrentChunkY);
            //Light.SetLightBlocks(chunkPrimer.arrayLightBlocks.ToArray());
            chunk.Light.GenerateHeightMap(); // 0.02 мс
            //InitHeightMapGen();
            //World.Filer.EndSectionLog();
        }





        public void Populate(ChunkBase chunk)
        {
           // Debug.Burden(1.5f);
        }
    }
}
