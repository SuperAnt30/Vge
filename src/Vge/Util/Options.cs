using System.IO;

namespace Vge.Util
{
    /// <summary>
    /// Объект настроек
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Префикс для пути игры
        /// </summary>
        public static string PrefixPath = "";
        /// <summary>
        /// путь к ресурсам
        /// </summary>
        public static string PathAssets = "Assets" + Path.DirectorySeparatorChar;
        /// <summary>
        /// Путь к сохранении игр
        /// </summary>
        public static string PathGames = "Games" + Path.DirectorySeparatorChar;

        #region Path

        /// <summary>
        /// Путь к папке звуков
        /// </summary>
        public static string PathSounds { get; private set; }
        /// <summary>
        /// Путь к папке шейдеров
        /// </summary>
        public static string PathShaders { get; private set; }
        /// <summary>
        /// Путь к папке текстур
        /// </summary>
        public static string PathTextures { get; private set; }
        /// <summary>
        /// Путь к папке данных блока
        /// </summary>
        public static string PathBlocks { get; private set; }
        /// <summary>
        /// Путь к папке фигур блока
        /// </summary>
        public static string PathShapeBlocks { get; private set; }

        #endregion

        

        /// <summary>
        /// Размер интерфеса с учётом размера окна
        /// </summary>
        public static int SizeInterface = 1;
        /// <summary>
        /// Полноэкранные окно или нет
        /// </summary>
        public static bool FullScreen = false;
        /// <summary>
        /// Вертикальная синхронизация
        /// </summary>
        public static bool VSync = true;
        /// <summary>
        /// Графика качественнее, красивее
        /// </summary>
        public static bool Qualitatively = true;
        /// <summary>
        /// Желаемый FPS
        /// </summary>
        public static int Fps = 60;

        /// <summary>
        /// Общая громкость
        /// </summary>
        public static int SoundVolume = 100;
        /// <summary>
        /// Получить громкость звуковых эффектов
        /// </summary>
        public static float SoundVolumeFloat { get; private set; }
        /// <summary>
        /// Громкость музыки
        /// </summary>
        public static int MusicVolume = 100;
        /// <summary>
        /// Получить громкость музыки
        /// </summary>
        public static float MusicVolumeFloat { get; private set; }

        /// <summary>
        /// Имя игрока
        /// </summary>
        public static string Nickname = "Nickname";
        /// <summary>
        /// Токен игрока, для сети
        /// </summary>
        public static string Token = "Token";

        /// <summary>
        /// Чувствительность мышки, 0 - 100, где 0 это минимум, 100 максимум, 50 середина
        /// </summary>
        public static int MouseSensitivity = 50;
        /// <summary>
        /// Получить чувствительность мыши
        /// </summary>
        public static float MouseSensitivityFloat { get; private set; }

        /// <summary>
        /// Обзор чанков
        /// </summary>
        public static byte OverviewChunk = 16;

        /// <summary>
        /// IP адрес сервера
        /// </summary>
        public static string IpAddress = "127.0.0.1";

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

            PathSounds = PathAssets + PrefixPath + "Sounds" + Path.DirectorySeparatorChar;
            PathShaders = PathAssets + PrefixPath + "Shaders" + Path.DirectorySeparatorChar;
            PathTextures = PathAssets + PrefixPath + "Textures" + Path.DirectorySeparatorChar;
            PathBlocks = PathAssets + PrefixPath + "Blocks" + Path.DirectorySeparatorChar;
            PathShapeBlocks = PathBlocks + "Shapes" + Path.DirectorySeparatorChar;

            // Gi.UpdateSizeInterface() тут не надо, так-как при загрузке после опции, 
            // будет OnResized(), и там вызывается Gi.UpdateSizeInterface()
        }
    }
}
