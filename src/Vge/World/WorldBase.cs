using Vge.Util;

namespace Vge.World
{
    /// <summary>
    /// Абстрактный объект мира
    /// </summary>
    public abstract class WorldBase
    {
        /// <summary>
        /// Это значение true для клиентских миров и false для серверных миров.
        /// </summary>
        public bool IsRemote { get; protected set; }
        /// <summary>
        /// Объект генератора случайных чисел
        /// </summary>
        public Rand Rnd { get; protected set; }
        /// <summary>
        /// Объект сыщик
        /// </summary>
        public Profiler Filer { get; protected set; }

        /// <summary>
        /// Такт сервера
        /// </summary>
        public virtual void Update()
        {

        }
    }
}
