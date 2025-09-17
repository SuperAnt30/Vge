using System;
using System.Runtime.CompilerServices;
using Vge.World;

namespace Vge.Entity.Render
{
    /// <summary>
    /// Базовый класс рендера для сущности, объект пустой, для сервера
    /// </summary>
    public class EntityRenderBase : IDisposable
    {
        /// <summary>
        /// Метод для прорисовки
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Draw(float timeIndex, float deltaTime) { }

        /// <summary>
        /// Обновить рассчитать матрицы для кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <param name="deltaTime">Дельта последнего кадра в mc</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void UpdateMatrix(float timeIndex, float deltaTime) { }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void UpdateClient(WorldClient world, float deltaTime) { }

        /// <summary>
        /// Добавить клип
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void AddClip(string key, float speed) { }

        /// <summary>
        /// Отменить клип
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void StopingClip(string key) { }

        /// <summary>
        /// Задать состояние открытости глаз
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetEyeOpen(bool value) { }

        /// <summary>
        /// Задать состояние рта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetMouthState(EnumMouthState value) { }

        /// <summary>
        /// Задать байт флагов анимации движения
        /// FBLRSnSp
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void SetMovingFlags(byte moving) { }

        /// <summary>
        /// Имеется ли сейчас движение только стрейф, без движения вперёд или назад или бездействия
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsMovingStrafe() => false;

        /// <summary>
        /// Изменён предмет внешности (что в руках или одежда)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void OutsideItemChanged() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Dispose() { }
    }
}
