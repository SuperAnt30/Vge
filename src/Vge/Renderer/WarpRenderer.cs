using System;
using Vge.Games;

namespace Vge.Renderer
{
    /// <summary>
    /// Базовый класс основы, для рендера
    /// </summary>
    public abstract class WarpRenderer : IDisposable
    {
        /// <summary>
        /// Класс  игры
        /// </summary>
        protected readonly GameBase _game;

        public WarpRenderer(GameBase game) => _game = game;

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public virtual void Draw(float timeIndex) { }

        public virtual void Dispose() { }
    }
}
