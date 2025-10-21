using System;
using Vge.Renderer;
using WinGL.Actions;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол кнопки только картинка, без текста
    /// </summary>
    public abstract class ButtonIcon : WidgetBase
    {
        /// <summary>
        /// Нажали ли на кнопку
        /// </summary>
        protected bool _isLeftDown;

        /// <summary>
        /// Сетка фона
        /// </summary>
        protected readonly MeshGuiColor _meshBg;

        public ButtonIcon(WindowMain window, int width, int height) : base(window)
        {
            SetSize(width, height);
            _meshBg = new MeshGuiColor(gl);
        }

        #region Draw

        public override void Rendering()
        {
            _RenderInside(window.Render, PosX * _si, PosY * _si);
            IsRender = false;
        }

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected virtual void _RenderInside(RenderMain render, int x, int y) { }

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем фон кнопки
            window.Render.BindTextureWidgets();
            _meshBg.Draw();
        }

        #endregion

        #region OnMouse

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left)
            {
                OnMouseMove(x, y);
                if (Enter)
                {
                    _isLeftDown = true;
                    IsRender = true;
                    // Звук клика
                    window.SoundClick(.3f);
                }
            }
        }

        /// <summary>
        /// Отпустили клавишу мышки
        /// </summary>
        public override void OnMouseUp(MouseButton button, int x, int y)
        {
            if (_isLeftDown)
            {
                if (button == MouseButton.Left)
                {
                    OnMouseMove(x, y);
                    if (Enter)
                    {
                        _OnClick();
                    }
                }
                _isLeftDown = false;
                IsRender = true;
            }
        }

        #endregion

        public override void Dispose() => _meshBg?.Dispose();

        #region Event

        /// <summary>
        /// Событие клика по кнопке
        /// </summary>
        public event EventHandler Click;
        /// <summary>
        /// Событие клика по кнопке
        /// </summary>
        protected void _OnClick() => Click?.Invoke(this, new EventArgs());

        #endregion
    }
}
