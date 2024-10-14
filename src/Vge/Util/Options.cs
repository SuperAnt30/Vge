using System.IO;

namespace Vge.Util
{
    /// <summary>
    /// Объект настроек
    /// </summary>
    public class Options
    {
        /// <summary>
        /// путь к ресурсам
        /// </summary>
        public static string PathAssets { get; set; } = "Assets" + Path.DirectorySeparatorChar;
        /// <summary>
        /// Путь к папке шейдеров
        /// </summary>
        public static string PathShaders { get; private set; }
        /// <summary>
        /// Путь к папке текстур
        /// </summary>
        public static string PathTextures { get; private set; }
        /// <summary>
        /// Путь к сохранении игр
        /// </summary>
        public static string PathGames { get; set; } = "Games" + Path.DirectorySeparatorChar;

        /// <summary>
        /// Размер интерфеса с учётом размера окна
        /// </summary>
        public static int SizeInterface { get; set; } = 1;
        /// <summary>
        /// Полноэкранные окно или нет
        /// </summary>
        public static bool FullScreen { get; set; } = false;
        /// <summary>
        /// Вертикальная синхронизация
        /// </summary>
        public static bool VSync { get; set; } = true;
        /// <summary>
        /// Желаемый FPS
        /// </summary>
        public static int Fps { get; set; } = 60;

        /// <summary>
        /// Общая громкость
        /// </summary>
        public static int SoundVolume { get; set; } = 100;
        /// <summary>
        /// Получить громкость звуковых эффектов
        /// </summary>
        public static float SoundVolumeFloat { get; private set; }
        /// <summary>
        /// Громкость музыки
        /// </summary>
        public static int MusicVolume { get; set; } = 100;
        /// <summary>
        /// Получить громкость музыки
        /// </summary>
        public static float MusicVolumeFloat { get; private set; }

        /// <summary>
        /// Имя игрока
        /// </summary>
        public static string Nickname { get; set; } = "Nickname";
        /// <summary>
        /// Токен игрока, для сети
        /// </summary>
        public static string Token { get; set; } = "Token";

        /// <summary>
        /// Чувствительность мышки, 0 - 100, где 0 это минимум, 100 максимум, 50 середина
        /// </summary>
        public static int MouseSensitivity { get; set; } = 50;
        /// <summary>
        /// Получить чувствительность мыши
        /// </summary>
        public static float MouseSensitivityFloat { get; private set; } = 50;

        /// <summary>
        /// Обзор чанков
        /// </summary>
        public static byte OverviewChunk { get; set; } = 16;

        /// <summary>
        /// IP адрес сервера
        /// </summary>
        public static string IpAddress { get; set; } = "127.0.0.1";

        /// <summary>
        /// Обновить данные переменных
        /// </summary>
        public static void UpData()
        {
            SoundVolumeFloat = SoundVolume / 100f;
            MusicVolumeFloat = MusicVolume / 100f;

            if (MouseSensitivity > 50)
            {
                MouseSensitivityFloat = 3f + (MouseSensitivity - 50) / 7f;
            }
            else
            {
                MouseSensitivityFloat = .5f + MouseSensitivity / 20f;
            }

            PathShaders = PathAssets + "Shaders" + Path.DirectorySeparatorChar;
            PathTextures = PathAssets + "Textures" + Path.DirectorySeparatorChar;

            // Gi.UpdateSizeInterface() тут не надо, так-как при загрузке после опции, 
            // будет OnResized(), и там вызывается Gi.UpdateSizeInterface()
        }
    }
}
