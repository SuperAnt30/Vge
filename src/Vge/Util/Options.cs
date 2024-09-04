using System.IO;
using Vge.Gui;

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
        /// Размер интерфеса с учётом размера окна
        /// </summary>
        public static int SizeInterface { get; set; } = 1;

        /// <summary>
        /// Общая громкость
        /// </summary>
        public static int SoundVolume { get; set; } = 100;
        /// <summary>
        /// Получить громкость звуковых эффектов
        /// </summary>
        public static float SoundVolumeFloat { get; private set; }

        /// <summary>
        /// Желаемый FPS
        /// </summary>
        public static int Fps { get; set; } = 60;

        /// <summary>
        /// Чувствительность мышки, 0 - 100, где 0 это минимум, 100 максимум, 50 середина
        /// </summary>
        public static int MouseSensitivity { get; set; } = 50;
        /// <summary>
        /// Получить чувствительность мыши
        /// </summary>
        public static float MouseSensitivityFloat { get; private set; } = 50;

        /// <summary>
        /// Обновить данные переменных
        /// </summary>
        public static void UpData()
        {
            SoundVolumeFloat = SoundVolume / 100f;
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
