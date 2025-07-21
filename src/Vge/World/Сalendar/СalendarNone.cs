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
        /// Обновление раз в тик на клиенте
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateClient() => TickCounter++;

        /// <summary>
        /// Обновление раз в тик на сервере
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateServer() => TickCounter++;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTickCounter(uint tickCounter) => TickCounter = tickCounter;

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
    }
}
