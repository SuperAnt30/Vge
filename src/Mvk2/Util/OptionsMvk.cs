using Vge.Util;

namespace Mvk2.Util
{
    public class OptionsMvk : Options
    {
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
        }
    }
}
