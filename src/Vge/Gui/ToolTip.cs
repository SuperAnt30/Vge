using System;
using Vge.Renderer;
using Vge.Renderer.Font;

namespace Vge.Gui
{
    /// <summary>
    /// Объект для скрина, для прорисовки всплывающей подсказки
    /// </summary>
    public class ToolTip : IDisposable
    {
        /// <summary>
        /// Объект окна
        /// </summary>
        protected readonly WindowMain _window;

        /// <summary>
        /// Текст подсказки
        /// </summary>
        private string _text;
        /// <summary>
        /// Объект шрифта
        /// </summary>
        private FontBase _font;
        /// <summary>
        /// Нужен ли рендер
        /// </summary>
        private bool _isRender;

        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;
        /// <summary>
        /// Сетка текста
        /// </summary>
        private readonly MeshGuiColor _meshTxt;

        public ToolTip(WindowMain window, FontBase font)
        {
            _window = window;
            _font = font;
            _meshBg = new MeshGuiColor(window.GetOpenGL());
            _meshTxt = new MeshGuiColor(window.GetOpenGL());
        }

        public void SetText(string text)
        {
            _text = text;
            _isRender = true;
            Show();
        }

        /// <summary>
        /// Показывать
        /// </summary>
        public void Show() { }

        /// <summary>
        /// Скрывать
        /// </summary>
        public void Hide() { }

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public void OnMouseMove(int x, int y) { }

        #region Draw

        /// <summary>
        /// рендер контрола
        /// </summary>
        protected virtual void _Rendering()
        {
        }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public virtual void Draw(float timeIndex)
        {
            if (_isRender)
            {
                _Rendering();
                _isRender = false;
            }
            _window.Render.BindTextureWidgets();
            _meshBg.Draw();
            _window.Render.ShaderBindGuiColor();
            _font.BindTexture();
            _meshTxt.Draw();
        }

        #endregion

        public void Dispose()
        {
            _meshBg?.Dispose();
            _meshTxt?.Dispose();
        }
    }
}
