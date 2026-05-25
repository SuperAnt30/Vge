using System.Runtime.CompilerServices;
using Vge.Entity;

namespace Mvk2.Entity
{
    /// <summary>
    /// Интерфейс наличия блоков в которой находится сущность
    /// </summary>
    public class PresenceBlocksMvk : IPresenceBlocks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInLiquid()
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInWater()
        {
            return false;
        }

        /// <summary>
        /// Обновление перерасчёта в такте
        /// </summary>
        public void Update()
        {

        }
    }
}
