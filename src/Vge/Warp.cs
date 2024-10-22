using System;
using Vge.Renderer;
using WinGL.Actions;
using WinGL.OpenGL;

namespace Vge
{
    /// <summary>
    /// Базовый класс основы, для объектов Screen, Game
    /// </summary>
    public abstract class Warp : IDisposable
    {
        /// <summary>
        /// Объект окна
        /// </summary>
        protected readonly WindowMain window;
        /// <summary>
        /// Объект OpenGL для элемента управления
        /// </summary>
        protected readonly GL gl;

        public Warp(WindowMain window)
        {
            this.window = window;
            gl = window.GetOpenGL();
        }

        /// <summary>
        /// Получить объект OpenGL. Использовать только в основном потоке
        /// </summary>
        public GL GetOpenGL() => gl;

        /// <summary>
        /// Объект отвечающий за прорисовку
        /// </summary>
        public RenderMain Render => window.Render;

        #region Draw

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public virtual void Draw(float timeIndex) { }

        #endregion

        #region Tick

        /// <summary>
        /// Игровой такт
        /// </summary>
        public virtual void OnTick(float deltaTime) { }

        #endregion

        #region OnMouse

        /// <summary>
        /// Перемещение мыши
        /// </summary>
        public virtual void OnMouseMove(int x, int y) { }
        /// <summary>
        /// Нажатие курсора мыши
        /// </summary>
        public virtual void OnMouseDown(MouseButton button, int x, int y) { }
        /// <summary>
        /// Отпустил курсор мыши
        /// </summary>
        public virtual void OnMouseUp(MouseButton button, int x, int y) { }
        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        public virtual void OnMouseWheel(int delta, int x, int y) { }

        #endregion

        #region OnKey

        /// <summary>
        /// Клавиша нажата
        /// </summary>
        public virtual void OnKeyDown(Keys keys) { }

        /// <summary>
        /// Клавиша отпущена
        /// </summary>
        public virtual void OnKeyUp(Keys keys) { }

        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        public virtual void OnKeyPress(char key) { }

        #endregion

        public virtual void Dispose() { }
    }
}
