namespace Vge.World.Gen.Layer
{
    /// <summary>
    /// Абстрактный класс слоёв
    /// https://github.com/Cubitect/cubiomes
    /// </summary>
    public abstract class GenLayer
    {
        /// <summary>
        /// Базовое семя для LCG, предоставленного через конструктор
        /// </summary>
        protected long _baseSeed;
        /// <summary>
        /// Родительский GenLayer, который был предоставлен через конструктор
        /// </summary>
        protected GenLayer _parent;

        /// <summary>
        /// Seed из мира, который используется в LCG
        /// </summary>
        private long _worldGenSeed;
        /// <summary>
        /// Заключительная часть LCG, которая использует координаты фрагмента X, Z
        /// вместе с двумя другими начальными значениями для генерации псевдослучайных чисел
        /// </summary>
        private long _chunkSeed;

        protected GenLayer() => _baseSeed = 1;

        public GenLayer(long baseSeed)
        {
            _baseSeed = baseSeed;
            _baseSeed *= _baseSeed * 6364136223846793005L + 1442695040888963407L;
            _baseSeed += baseSeed;
            _baseSeed *= _baseSeed * 6364136223846793005L + 1442695040888963407L;
            _baseSeed += baseSeed;
            _baseSeed *= _baseSeed * 6364136223846793005L + 1442695040888963407L;
            _baseSeed += baseSeed;
        }

        /// <summary>
        /// Инициализировать локальный worldGenSeed слоя на основе его собственного baseSeed
        /// и глобального начального числа (передается в качестве аргумента)
        /// </summary>
        public virtual void InitWorldGenSeed(long worldSeed)
        {
            _worldGenSeed = worldSeed;
            _parent?.InitWorldGenSeed(worldSeed);
            _worldGenSeed *= _worldGenSeed * 6364136223846793005L + 1442695040888963407L;
            _worldGenSeed += _baseSeed;
            _worldGenSeed *= _worldGenSeed * 6364136223846793005L + 1442695040888963407L;
            _worldGenSeed += _baseSeed;
            _worldGenSeed *= _worldGenSeed * 6364136223846793005L + 1442695040888963407L;
            _worldGenSeed += _baseSeed;
        }

        /// <summary>
        /// Инициализировать текущий chunkSeed слоя на основе локального worldGenSeed 
        /// и координат чанка (x,z)
        /// </summary>
        public void InitChunkSeed(long x, long y)
        {
            _chunkSeed = _worldGenSeed;
            _chunkSeed *= _chunkSeed * 6364136223846793005L + 1442695040888963407L;
            _chunkSeed += x;
            _chunkSeed *= _chunkSeed * 6364136223846793005L + 1442695040888963407L;
            _chunkSeed += y;
            _chunkSeed *= _chunkSeed * 6364136223846793005L + 1442695040888963407L;
            _chunkSeed += x;
            _chunkSeed *= _chunkSeed * 6364136223846793005L + 1442695040888963407L;
            _chunkSeed += y;
        }

        /// <summary>
        /// Возвращает список целочисленных значений, сгенерированных этим слоем. 
        /// Их можно интерпретировать как температуру, 
        /// количество осадков или индексы biomeList[], 
        /// основанные на конкретном подклассе GenLayer.
        /// </summary>
        public abstract int[] GetInts(int areaX, int areaZ, int width, int height);

        /// <summary>
        /// Возвращает псевдослучайное число LCG из [0, x). Аргументы: целое х
        /// </summary>
        protected int _NextInt(int x)
        {
            int random = (int)((_chunkSeed >> 24) % x);
            if (random < 0) random += x;

            _chunkSeed *= _chunkSeed * 6364136223846793005L + 1442695040888963407L;
            _chunkSeed += _worldGenSeed;
            return random;
        }

        /// <summary>
        /// Возвращает псевдослучайное число LCG из [0, 1]
        /// </summary>
        protected bool _NextBool() => _NextInt(2) == 1;
    }
}
