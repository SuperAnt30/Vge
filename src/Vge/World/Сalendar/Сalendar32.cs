using System;
using System.Runtime.CompilerServices;
using Vge.Util;
using WinGL.Util;

namespace Vge.World.Сalendar
{
    /// <summary>
    /// Мировой календарь на 32 дня в году
    /// </summary>
    public class Сalendar32 : IСalendar
    {
        
        /// <summary>
        /// Скорость года в днях
        /// </summary>
        private const byte _speedYear = 32;
        /// <summary>
        /// Яркость луны по фазам
        /// </summary>
        private readonly static float[] _lightMoonPhase = new float[] { 0, .16f, .32f, .48f, .8f, .48f, .32f, .16f };

        /// <summary>
        /// Угол солнца, от пары года
        /// </summary>
        public readonly static float[] AngleSunTimeYear = new float[] { Glm.Pi45, Glm.Pi20, Glm.Pi45, Glm.Pi60 };

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
        /// Пара года
        /// </summary>
        public int TimeYearIndex { get; private set; }
        /// <summary>
        /// Фаза луны тип
        /// </summary>
        public EnumMoonPhase MoonPhase { get; private set; }
        /// <summary>
        /// Фаза луны индекс
        /// </summary>
        public int MoonPhaseIndex { get; private set; }
        /// <summary>
        /// Яркость звёзд 0.0 - 0.75
        /// </summary>
        public float StarLight { get; private set; }
        /// <summary>
        /// Цвет облаков
        /// </summary>
        public Vector3 ColorClouds = new Vector3(.9f);
        /// <summary>
        /// Коэффициент количество облаков
        /// Ливень = .31f
        /// Дождь = .41f
        /// Пасмурно = .51f
        /// Облачно = .61f
        /// Мало облачно = .76f
        /// Ясно = .91f
        /// </summary>
        public float FewClouds { get; private set; } = .91f;
        /// <summary>
        /// Скорость облаков по X
        /// </summary>
        public float SpeedCloudX { get; private set; } = .0625f; // 0.0625f; // .125f;
        /// <summary>
        /// Скорость облаков по Z
        /// </summary>
        public float SpeedCloudZ { get; private set; } = .03125f;
        /// <summary>
        /// Состояние облаков
        /// </summary>
        public EnumClouds CloudConditions { get; private set; } = EnumClouds.Clear;
        /// <summary>
        /// Состояние облаков следующего такта
        /// </summary>
        private EnumClouds _cloudConditionsNext = EnumClouds.Clear;

        /// <summary>
        /// Скорость суток в тактах
        /// </summary>
        private readonly int _speedDay;
        /// <summary>
        /// Время конкретного дня, в тактах. 
        /// 0 это полдень, 
        /// </summary>
        private int _time;
        /// <summary>
        /// Яркость солнца
        /// </summary>
        private float _sunLight;
        /// <summary>
        /// Небесный угол
        /// </summary>
        private float _celestialAngle;
        /// <summary>
        /// Нормализованный вектор источника света
        /// </summary>
        private Vector3 _vectorLight;
        /// <summary>
        /// Цвет неба
        /// </summary>
        private Vector3 _colorSky = new Vector3(.6f);
        /// <summary>
        /// Цвет тумана
        /// </summary>
        private Vector3 _colorFog = new Vector3(.2f);
        
        /// <summary>
        /// Угол солнца и луны прошлого кадра
        /// </summary>
        private float _angleSunPrev;
        /// <summary>
        /// Небесный свет ночь 0..3 в зависимости от фазы луны, день 15
        /// </summary>
        private int _skylightSubtracted;
        /// <summary>
        /// Значение ветра
        /// </summary>
        private float _wind;

        public Сalendar32(int speedDay)
        {
            _speedDay = speedDay;
            SetTickCounter((uint)(speedDay * 3 / 4));
        }

        /// <summary>
        /// Сколько игровых тактов длится день
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSpeedDay() => _speedDay;

        int _cloudDebug = 0;

        /// <summary>
        /// Обновление раз в тик на клиенте
        /// </summary>
        public void UpdateClient()
        {
            TickCounter++;

            if (++_cloudDebug > 100)
            {
                _cloudDebug = 0;

                //_cloudConditionsNext = CloudConditions + 1;
                //if ((int)_cloudConditionsNext == CloudConditionsConvert.CountEnumClouds)
                //{
                //    _cloudConditionsNext = 0;
                //}
                //_cloudConditionsNext = EnumClouds.Cloudy;
                //Rand rand = new Rand();
                //_cloudConditionsNext = (EnumClouds)rand.Next(CloudConditionsConvert.CountEnumClouds);
            }

            // Плавность смены облаков
            if (_cloudConditionsNext != CloudConditions)
            {
                float fewClouds = CloudConditionsConvert.FewClouds(_cloudConditionsNext);

                if (fewClouds == FewClouds) CloudConditions = _cloudConditionsNext;
                else if (fewClouds < FewClouds)
                {
                    FewClouds -= .005f;
                    if (fewClouds >= FewClouds)
                    {
                        CloudConditions = _cloudConditionsNext;
                        FewClouds = fewClouds;
                    }
                }
                else
                {
                    FewClouds += .005f;
                    if (fewClouds <= FewClouds)
                    {
                        CloudConditions = _cloudConditionsNext;
                        FewClouds = fewClouds;
                    }
                }
            }

            _CalculateInitialYear();

            // Находим угол солнца и луны в небе относительно заданного времени (0.0 - 1.0)
            float angleSun = _celestialAngle * Glm.Pi360;
            //Console.WriteLine(angleSun);

            if (_angleSunPrev != angleSun)
            {
                _angleSunPrev = angleSun;

                // Углы амплитуд косинуса и синуса
                float lightCos = Glm.Cos(angleSun) * 2f;
                //float lightSin = Glm.Sin(angleSun) * 2f;

                // Яркость солнца
                _sunLight = Mth.Clamp(lightCos + .64f, 0, 1);
                // Яркость неба
                float skyLight = Mth.Clamp(lightCos + .5f, 0, 1);

                // Яркость звёзд 0.0 - 0.75
                StarLight = Mth.Clamp(1f - lightCos + .25f, 0, 1);
                StarLight = StarLight * StarLight * .75f;

                Mat4 matSun = Mat4.Identity();
                // Вектор солнцы или луны
                if (_celestialAngle > .25f && _celestialAngle < .75f)// lightSin < 0)
                {
                    // Ночь
                    matSun.RotateX(Glm.Pi45);
                    matSun.RotateZ(angleSun);
                    _vectorLight = new Vector3(matSun * new Vector4(0, -1, 0, 1));

                }
                else
                {
                    // День
                    matSun.RotateX(AngleSunTimeYear[TimeYearIndex]);
                    matSun.RotateZ(angleSun);
                    _vectorLight = new Vector3(matSun * new Vector4(0, 1, 0, 1));
                }

                // Цвет неба
                _colorSky.X = .5f * skyLight;
                _colorSky.Y = .7f * skyLight;
                _colorSky.Z = .99f * skyLight;

                // Цвет тумана
                _colorFog.X = .71f * skyLight + .06f;
                _colorFog.Y = .8f * skyLight + .06f;
                _colorFog.Z = .91f * skyLight + .09f;

                // Цвет облаков
                ColorClouds.X = .9f * skyLight + .1f;
                ColorClouds.Y = .9f * skyLight + .1f;
                ColorClouds.Z = .85f * skyLight + .15f;
            }
        }

        /// <summary>
        /// Рассчитать год, пору года и день в году
        /// </summary>
        private void _CalculateInitialYear()
        {
            _time++;
            if (_time >= _speedDay)
            {
                // Изменился день
                _time = 0;
                Day++;
                if (Day >= _speedYear)
                {
                    Day = 0;
                    Year++;
                }
                TimeYearIndex = Day / 8;
                TimeYear = (EnumTimeYear)TimeYearIndex;
                MoonPhaseIndex = (Day + 4) % 8;
                MoonPhase = (EnumMoonPhase)MoonPhaseIndex;
            }

            // рассчитать небесный свет
            _celestialAngle = _CalculateCelestialAngle();
            float light = 1f - (Glm.Cos(_celestialAngle * Glm.Pi360) * 2f + .5f);
            light = Mth.Clamp(light, 0f, 1f);
            light = light * (1f - _lightMoonPhase[MoonPhaseIndex] * .3125f);
            light = light * (1f - .32f * .3125f);
            light = 1f - light;
            _skylightSubtracted = (int)(light * 15f);
        }

        /// <summary>
        /// Вычисляет угол солнца и луны в небе относительно заданного времени (0.0 - 1.0)
        /// </summary>
        private float _CalculateCelestialAngle()
        {
            /**
             * 0.00 = полдень
             * 0.25 = вечер
             * 0.50 = ночь
             * 0.75 = утро
             */
            float angleSun = _time / (float)_speedDay; // -.25 это чтоб с утра, 0 это день.
            if (angleSun < 0f) angleSun++;
            if (angleSun > 1f) angleSun--;

            float time2 = angleSun;
            angleSun = 1f - ((Glm.Cos(angleSun * Glm.Pi) + 1f) / 2f);

            // Длинна дня меняется от пары года
            if (TimeYear == EnumTimeYear.Winter)
            {
                angleSun = time2 - (angleSun - time2) / 2f; // 12 ночь - 8 день
            }
            else if (TimeYear == EnumTimeYear.Summer)
            {
                angleSun = time2 + (angleSun - time2); // 6.5 ночь - 13.5 день
            }
            else
            {
                angleSun = time2 + (angleSun - time2) / 2f; // 8 ночь - 12 день
            }

            //angleSun = time2 - (angleSun - time2) / 3f; // ~1 ночь - ~1 день просто реще смена заката
            //angleSun = time2 + (angleSun - time2) / 2f; 25 ночь - 35 день
            //angleSun = time2 + (angleSun - time2);// 1 ночь - 2 день
            //angleSun = time2 - (angleSun - time2);// 2 ночь - 1 день
            return angleSun;
        }

        /// <summary>
        /// Обновление во фрейме, и возвращает было ли изменение
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public void UpdateFrame(float timeIndex)
        {
            _wind = Glm.Cos(((TickCounter & 0x7F) + timeIndex) * .049f) // Тут определяем скорость амплитуды
                * .16f; // Тут размер амплитуды, к 0 не двигается, 1 много
        }

        /// <summary>
        /// Обновление раз в тик на сервере
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateServer()
        {
            TickCounter++;
            _CalculateInitialYear();
        }

        public void SetTickCounter(uint tickCounter)
        {
            if (TickCounter != tickCounter || TickCounter == 0)
            {
                TickCounter = tickCounter;
                int day = (int)((TickCounter + 0) / _speedDay);
                _time = (int)(TickCounter - day * _speedDay);
                Day = (byte)(day % 32);
                Year = day / 32;
                TimeYearIndex = Day / 8;
                TimeYear = (EnumTimeYear)TimeYearIndex;
                MoonPhaseIndex = (Day + 4) % 8;
                MoonPhase = (EnumMoonPhase)MoonPhaseIndex;
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
        public float GetMoonLight() => _lightMoonPhase[MoonPhaseIndex];

        /// <summary>
        /// Получить небесный угол
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetCelestialAngle() => _celestialAngle;

        /// <summary>
        /// Получить небесныц свет ночь 0..3 в зависимости от фазы луны, день 15
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSkylightSubtracted() => _skylightSubtracted;

        /// <summary>
        /// Получить цвет неба
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetColorSky() => _colorSky;

        /// <summary>
        /// Получить цвет тумана
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetColorFog() => _colorFog;

        /// <summary>
        /// Значение ветра
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetWind() => _wind;

        /// <summary>
        /// Проверяет, является ли сейчас дневное время, определяя по яркости неба
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDayTime() => _skylightSubtracted > 6;

        public override string ToString()
            => _time + " d:" + Day + " y:" + Year 
            + " Sky:" + _skylightSubtracted + "/" + _sunLight
            + " " + TimeYear + " " + MoonPhase;
    }
}
