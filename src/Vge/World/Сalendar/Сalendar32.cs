using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.World.Сalendar
{
    /// <summary>
    /// Мировой календарь на 32 дня в году
    /// </summary>
    public class Сalendar32 : IСalendar
    {
        /// <summary>
        /// Скорость суток в тактах
        /// </summary>
        public readonly ushort SpeedDay;
        /// <summary>
        /// Скорость года в днях
        /// </summary>
        private const byte _speedYear = 32;

        /// <summary>
        /// Увеличивается каждый игровой тик
        /// </summary>
        public uint TickCounter { get; private set; }

        /// <summary>
        /// Игровой год
        /// </summary>
        public int Year { get; private set; }
        /// <summary>
        /// Порядковый номер дня в году, 0-31
        /// </summary>
        public byte Day { get; private set; }
        /// <summary>
        /// Пара года
        /// </summary>
        public EnumTimeYear TimeYear { get; private set; }
        /// <summary>
        /// Фаза луны
        /// </summary>
        public EnumMoonPhase MoonPhase { get; private set; }
        /// <summary>
        /// Время конкретного дня, в тактах
        /// </summary>
        private ushort _time;
        /// <summary>
        /// Яркость солнца
        /// </summary>
        private float _sunLight;
        /// <summary>
        /// Яркость неба
        /// </summary>
        private float _skyLight;
        /// <summary>
        /// Яркость звёзд
        /// </summary>
        private float _starLight;
        /// <summary>
        /// Нормализованный вектор источника света
        /// </summary>
        private Vector3 _vectorLight;

        public Сalendar32(ushort speedDay) => SpeedDay = speedDay;

        /// <summary>
        /// Обновление раз в тик на клиенте
        /// </summary>
        public void UpdateClient()
        {
            TickCounter++;
            _time++;
            if (_time > SpeedDay)
            {
                // Изменился день
                _time = 0;
                Day++;
                if (Day > _speedYear)
                {
                    Day = 0;
                    Year++;
                }
                TimeYear = (EnumTimeYear)(Day / 8);
                MoonPhase = (EnumMoonPhase)((Day + 4) % 8);
            }
            // Надо просчитать расположения солнца и луны и прочего

            // Находим угол солнца и луны в небе относительно заданного времени (0.0 - 1.0)
            float angleSun = _time / (float)SpeedDay - .25f;
            if (angleSun < 0f) angleSun++;
            if (angleSun > 1f) angleSun--;
            float time2 = angleSun;
            angleSun = 1f - ((Glm.Cos(angleSun * Glm.Pi) + 1f) / 2f);
            angleSun = time2 + (angleSun - time2) / 3f;
            angleSun *= Glm.Pi360;

            // Углы амплитуд косинуса и синуса
            float lightCos = Glm.Cos(angleSun) * 2f;
            float lightSin = Glm.Sin(angleSun) * 2f;

            // Яркость солнца
            _sunLight = Mth.Clamp(lightSin + .64f, 0, 1);
            // Яркость неба
            _skyLight = Mth.Clamp(lightSin + .5f, 0, 1);

            if (lightSin < 0)
            {
                // Яркость звёзд 0.0 - 0.75
                _starLight = Mth.Clamp(lightSin + .25f, 0, 1);
                _starLight = _starLight * _starLight * .75f;

                // Ночь
                lightSin = -lightSin;
                lightCos = -lightCos;
            }
            else
            {
                // День
            }

            // Вектор солнцы или луны
            _vectorLight = new Vector3(lightCos, lightSin * 4f, lightSin * 2f).Normalize();
        }

        /// <summary>
        /// Обновление раз в тик на сервере
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateServer() => TickCounter++;

        public void SetTickCounter(uint tickCounter)
        {
            if (TickCounter != tickCounter)
            {
                TickCounter = tickCounter;
                int day = (int)((TickCounter + 0) / SpeedDay);
                _time = (ushort)(TickCounter - day * SpeedDay);
                Day = (byte)(day % 32);
                Year = day / 32;
                TimeYear = (EnumTimeYear)(Day / 8);
                MoonPhase = (EnumMoonPhase)((Day + 4) % 8);
            }
        }

        /// <summary>
        /// Получить нормализованный вектор источника света
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetVectorLight() => _vectorLight;

        /// <summary>
        /// Получить яркость солнца
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSunLight() => _sunLight;

        /// <summary>
        /// Получить яркость луны
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetMoonLight() => .25f;

    }
}
