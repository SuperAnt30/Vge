using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.World.Сalendar
{
    /// <summary>
    /// Мировой календарь без учёта дней
    /// </summary>
    public class СalendarNone : IСalendar
    {
        /// <summary>
        /// Увеличивается каждый игровой тик
        /// </summary>
        public uint TickCounter { get; private set; }

        /// <summary>
        /// Нормализованный вектор источника света
        /// </summary>
        private readonly Vector3 _vectorLight = new Vector3(-1, 8, 1).Normalize();
        /// <summary>
        /// Цвет неба
        /// </summary>
        private readonly Vector3 _colorSky = new Vector3(.6f);
        /// <summary>
        /// Цвет тумана
        /// </summary>
        private readonly Vector3 _colorFog = new Vector3(.2f);

        /// <summary>
        /// Пара года
        /// </summary>
        public EnumTimeYear TimeYear { get; private set; } = EnumTimeYear.None;

        /// <summary>
        /// Обновление раз в тик на клиенте
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateClient() => TickCounter++;

        /// <summary>
        /// Обновление раз в тик на сервере
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateServer() => TickCounter++;

        /// <summary>
        /// Обновление во фрейме, и возвращает было ли изменение
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool UpdateFrame(float timeIndex) => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTickCounter(uint tickCounter) => TickCounter = tickCounter;

        /// <summary>
        /// Сколько игровых тактов длится день
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSpeedDay() => 0;

        /// <summary>
        /// Получить нормализованный вектор источника света
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetVectorLight() => _vectorLight;

        /// <summary>
        /// Получить яркость солнца, 0.0 - 1.0
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSunLight() => 1;

        /// <summary>
        /// Получить яркость луны, 0.0 - 0.5
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetMoonLight() => .12f;

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

        public override string ToString() => "";
    }
}
