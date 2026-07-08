using System.Runtime.CompilerServices;
using Vge.Util;

namespace Vge.Entity.AI
{
    /// <summary>
    /// Базовый объект задачи искусственного интеллекта моба
    /// </summary>
    public abstract class EntityAIBase
    {
        /// <summary>
        /// Сущность которая наблюдает, т.е. эта
        /// </summary>
        protected readonly EntityMob _entity;

        /// <summary>
        /// Устанавливает битовую маску, указывающую, какие другие задачи не могут выполняться одновременно.
        /// Тест представляет собой простое побитовое И — если он дает ноль, две задачи могут
        /// выполняться одновременно, если нет — они должны запускаться исключительно друг от друга.
        /// 0001 = 1 движение
        /// 0010 = 2 вращение
        /// 0011 = 3 движение и вращение
        /// 0100 = 4 прыжок
        /// 0101 = 5 движение и прыжок
        /// 0110 = 6 вращение и прыжок
        /// 0111 = 7 движение и вращение и прыжок
        /// </summary>
        public readonly int MutexBits;

        /// <summary>
        /// Объект генератора случайных чисел
        /// </summary>
        public readonly Rand Rnd;

        protected EntityAIBase(EntityMob entity, int mutexBits = 0)
        {
            _entity = entity;
            Rnd = _entity.GetWorldServer().Rnd;
            MutexBits = mutexBits;
        }

        /// <summary>
        /// Сбрасывает задачу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void ResetTask() { }

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void UpdateTask() { }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public abstract bool ShouldExecute();

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void StartExecuting() { }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool ContinueExecuting() => ShouldExecute();

        /// <summary>
        /// Определите, может ли эта задача ИИ быть прервана задачей с более высоким приоритетом. 
        /// У всех ванильных AITask это значение равно true.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsInterruptible() => true;
    }
}
