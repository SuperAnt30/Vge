using System.Runtime.CompilerServices;
using System.Threading;

namespace Vge.Util
{
    /// <summary>
    /// Заглушка, для простых задачь в разных потоках
    /// </summary>
    public struct WeaverSpinLock
    {
        private long _value;

        /// <summary>
        /// Ставим замок на выполнение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Lock()
        {
            while (Interlocked.Exchange(ref _value, 1) != 0) ;
        }

        /// <summary>
        /// Снимаем замок после выполнения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unlock()
        {
            Interlocked.Exchange(ref _value, 0);
        }
    }
}
