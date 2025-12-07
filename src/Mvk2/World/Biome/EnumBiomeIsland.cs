using WinGL.Util;

namespace Mvk2.World.Biome
{
    /// <summary>
    /// Перечень биомов для мира остров
    /// </summary>
    public enum EnumBiomeIsland
    {
        /// <summary>
        /// Море
        /// </summary>
        Sea = 0,
        /// <summary>
        /// Река
        /// </summary>
        River = 1,
        /// <summary>
        /// Равнина
        /// </summary>
        Plain = 2,
        /// <summary>
        /// Пустяня
        /// </summary>
        Desert = 3,
        /// <summary>
        /// Пляж
        /// </summary>
        Beach = 4,
        /// <summary>
        /// Сешанный лес
        /// </summary>
        MixedForest = 5,
        /// <summary>
        /// Хвойный лес
        /// </summary>
        ConiferousForest = 6,
        /// <summary>
        /// Берёзовый лес
        /// </summary>
        BirchForest = 7,
        /// <summary>
        /// Тропики
        /// </summary>
        Tropics = 8,
        /// <summary>
        /// Болото
        /// </summary>
        Swamp = 9,
        /// <summary>
        /// Горы
        /// </summary>
        Mountains = 10,
        /// <summary>
        /// Горы в пустыне
        /// </summary>
        MountainsDesert = 11
    }

    /// <summary>
    /// Объект статических методов и данных для биомов
    /// </summary>
    public sealed class Biomes
    {
        /// <summary>
        /// Массив цветов биома травы
        /// </summary>
        public static readonly Vector3[] ColorsGrass = new Vector3[]
        {
            new Vector3(.56f, .73f, .35f),
            new Vector3(.56f, .73f, .35f),
            new Vector3(.56f, .73f, .35f),
            new Vector3(.96f, .73f, .35f),
            new Vector3(.56f, .73f, .35f),
            new Vector3(.46f, .63f, .25f),
            new Vector3(.38f, .60f, .20f),
            new Vector3(.46f, .63f, .25f),
            new Vector3(.96f, .73f, .35f),
            new Vector3(.56f, .63f, .35f),
            new Vector3(.56f, .73f, .35f),
            new Vector3(.96f, .73f, .35f)
        };

        /// <summary>
        /// Массив цветов биома воды
        /// </summary>
        public static readonly Vector3[] ColorsWater = new Vector3[]
        {
            new Vector3(.1f, .35f, .7f),
            new Vector3(.24f, .45f, .88f),
            new Vector3(.24f, .45f, .88f),
            new Vector3(.24f, .45f, .88f),
            new Vector3(.24f, .45f, .88f),
            new Vector3(.24f, .45f, .88f),
            new Vector3(.24f, .45f, .88f),
            new Vector3(.24f, .45f, .88f),
            new Vector3(.24f, .45f, .88f),
            new Vector3(.36f, .63f, .68f),
            new Vector3(.24f, .45f, .88f),
            new Vector3(.24f, .45f, .88f)
        };
    }
}
