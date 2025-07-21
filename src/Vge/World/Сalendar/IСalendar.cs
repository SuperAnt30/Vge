using WinGL.Util;

namespace Vge.World.Сalendar
{
    /// <summary>
    /// Мировой календарь
    /// </summary>
    public interface IСalendar
    {
        /// <summary>
        /// Увеличивается каждый игровой тик
        /// </summary>
        uint TickCounter { get; }

        /// <summary>
        /// Обновление раз в тик на клиенте
        /// </summary>
        void UpdateClient();

        /// <summary>
        /// Обновление раз в тик на сервере
        /// </summary>
        void UpdateServer();

        /// <summary>
        /// Обновление во фрейме, и возвращает было ли изменение
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        bool UpdateFrame(float timeIndex);

        /// <summary>
        /// Внести изменение по мировому времени
        /// </summary>
        void SetTickCounter(uint tickCounter);

        /// <summary>
        /// Сколько игровых тактов длится день
        /// </summary>
        int GetSpeedDay();

        /// <summary>
        /// Получить нормализованный вектор источника света
        /// </summary>
        Vector3 GetVectorLight();

        /// <summary>
        /// Получить яркость солнца, 0.0 - 1.0
        /// </summary>
        float GetSunLight();

        /// <summary>
        /// Получить яркость луны, 0.0 - 0.5
        /// </summary>
        float GetMoonLight();

        /// <summary>
        /// Получить цвет неба
        /// </summary>
        Vector3 GetColorSky();

        /// <summary>
        /// Получить цвет тумана
        /// </summary>
        Vector3 GetColorFog();
    }
}
