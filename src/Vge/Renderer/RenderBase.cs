using WinGL.OpenGL;

namespace Vge.Renderer
{
    /// <summary>
    /// Базовый класс отвечающий за прорисовку
    /// </summary>
    public abstract class RenderBase
    {
        /// <summary>
        /// Объект окна
        /// </summary>
        protected readonly WindowMain window;
        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        protected readonly GL gl;

        public RenderBase(WindowMain window)
        {
            this.window = window;
            gl = window.GetOpenGL();
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public virtual void Draw(float timeIndex) { }

        /// <summary>
        /// Игровой такт
        /// </summary>
        public virtual void OnTick(float deltaTime) { }
    }
}
