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
        public static string PathAsets { get; set; } = "Assets\\";
        /// <summary>
        /// Общая громкость
        /// </summary>
        public static int SoundVolume { get; set; } = 100;
        /// <summary>
        /// Желаемый FPS
        /// </summary>
        public static int Fps { get; set; } = 60;
        /// <summary>
        /// Чувствительность мышки, 0 - 100, где 0 это минимум, 100 максимум, 50 середина
        /// </summary>
        public static int MouseSensitivity { get; set; } = 50;

        private static float soundVolume;
        private static float mouseSensitivity;

        /// <summary>
        /// Получить громкость звуковых эффектов
        /// </summary>
        public static float ToFloatSoundVolume() => soundVolume;
        /// <summary>
        /// Получить чувствительность мыши
        /// </summary>
        public static float ToFloatMouseSensitivity() => mouseSensitivity;
        /// <summary>
        /// Путь к папке шейдеров
        /// </summary>
        public static string ToPathShaders() => PathAsets + "Shaders\\";

        /// <summary>
        /// Обновить переменные float
        /// </summary>
        public static void UpFloat()
        {
            soundVolume = SoundVolume / 100f;
            if (MouseSensitivity > 50)
            {
                mouseSensitivity = 3f + (MouseSensitivity - 50) / 7f;
            }
            else
            {
                mouseSensitivity = .5f + MouseSensitivity / 20f;
            }
        }
    }
}
