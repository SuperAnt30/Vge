using System.IO;
using Vge.Util;

namespace Mvk2.Util
{
    public class OptionsMvk
    {
        /// <summary>
        /// Путь к папке шейдеров
        /// </summary>
        public static string PathShaders { get; private set; }
        /// <summary>
        /// Путь к папке звуков
        /// </summary>
        public static string PathSounds { get; private set; }
        /// <summary>
        /// Путь к папке текстур
        /// </summary>
        public static string PathTextures { get; private set; }

        /// <summary>
        /// Громкость музыки
        /// </summary>
        public static int MusicVolume { get; set; } = 100;
        /// <summary>
        /// Получить громкость музыки
        /// </summary>
        public static float MusicVolumeFloat { get; private set; }

        /// <summary>
        /// Обзор чанков
        /// </summary>
        public static int OverviewChunk { get; set; } = 16;

        /// <summary>
        /// Имя игрока
        /// </summary>
        public static string Nickname { get; set; } = "Nickname";

        /// <summary>
        /// IP адрес сервера
        /// </summary>
        public static string IpAddress { get; set; } = "127.0.0.1";

        /// <summary>
        /// Плавное освещение
        /// </summary>
        public static bool SmoothLighting { get; set; } = true;

        /// <summary>
        /// Обновить данные переменных
        /// </summary>
        public static void UpDataMvk()
        {
            MusicVolumeFloat = MusicVolume / 100f;

            string path = Options.PathAssets;
            PathShaders = path + "Mvk2Shaders" + Path.DirectorySeparatorChar;
            PathSounds = path + "Mvk2Sounds" + Path.DirectorySeparatorChar;
            PathTextures = path + "Mvk2Textures" + Path.DirectorySeparatorChar;
        }
    }
}
