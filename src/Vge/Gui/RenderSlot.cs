using System;
using System.Runtime.CompilerServices;
using Vge.Item;

namespace Vge.Gui
{
    /// <summary>
    /// Рендер слота
    /// </summary>
    public class RenderSlot : IDisposable
    {
        /// <summary>
        /// Стак
        /// </summary>
        public ItemStack Stack;
        /// <summary>
        /// Позиция X
        /// </summary>
        public int PosX;
        /// <summary>
        /// Позиция Y
        /// </summary>
        public int PosY;

        /// <summary>
        /// Размер интерфеса
        /// </summary>
        protected int _si = 1;

        /// <summary>
        /// Объект окна
        /// </summary>
        protected readonly WindowMain _window;

        public RenderSlot(WindowMain window, ItemStack stack)
        {
            _window = window;
            _si = Gi.Si;
            Stack = stack;
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnResized()
        {
            if (_si != Gi.Si)
            {
                _si = Gi.Si;
            }
        }

        public virtual void Rendering() { }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        public virtual void Draw() { }

        public virtual void Dispose() { }
    }
}
