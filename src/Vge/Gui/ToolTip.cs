using System;
using System.Configuration;
using Vge.Renderer;
using Vge.Renderer.Font;
using WinGL.Util;

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
        /// Видимость
        /// </summary>
        private bool _isVisible;

        protected float _mouseX;
        protected float _mouseY;

        private readonly int _biasX;
        private readonly int _biasY;

        /// <summary>
        /// Размер текста
        /// </summary>
        protected Vector2i _sizeText;

        /// <summary>
        /// Сетка текста
        /// </summary>
        private readonly MeshGuiColor _meshTxt;

        public ToolTip(WindowMain window, FontBase font, int biasX, int biasY)
        {
            _sizeText = new Vector2i(0);
            _window = window;
            _font = font;
            _biasX = biasX;
            _biasY = biasY;
            _meshTxt = new MeshGuiColor(window.GetOpenGL());
        }

        public void SetText(string text)
        {
            if (text == "")
            {
                Hide();
            }
            else
            {
                _text = text;
                _isRender = true;
                Show();
            }
        }

        /// <summary>
        /// Показывать
        /// </summary>
        public void Show() => _isVisible = true;

        /// <summary>
        /// Скрывать
        /// </summary>
        public void Hide() => _isVisible = false;

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public void OnMouseMove(int x, int y)
        {
            _mouseX = x + _biasX * Gi.Si;
            _mouseY = y + _biasY * Gi.Si;
        }

        #region Draw

        /// <summary>
        /// Рендер контрола
        /// </summary>
        protected virtual void _Rendering()
        {
            // Чистим буфер
            _font.Clear();
            // Указываем опции
            _font.SetFontFX(EnumFontFX.None);
            // Готовим рендер текста
            _sizeText = _font.RenderText(0, 0, _text);
            // Имеется Outline значит рендерим FX
            //_font.RenderFX();
            // Вносим сетку
            _font.Reload(_meshTxt);
        }

        /// <summary>
        /// Прорисовка кадра
        /// </summary>
        protected virtual void _Draw()
        {
            _window.Render.ShaderBindGuiColor(_mouseX, _mouseY);
            _font.BindTexture();
            _meshTxt.Draw();
            _window.Render.ShaderBindGuiColor(0, 0);
        }

        /// <summary>
        /// Прорисовки кадра
        /// </summary>
        public void Draw()
        {
            if (_isVisible)
            {
                if (_isRender)
                {
                    _Rendering();
                    _isRender = false;
                }
                _Draw();
            }
        }

        #endregion

        public virtual void Dispose() => _meshTxt?.Dispose();
    }
}
