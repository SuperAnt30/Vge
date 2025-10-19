using Vge.Realms;
using Vge.Renderer;
using Vge.Renderer.Font;
using WinGL.Actions;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол кнопки
    /// </summary>
    public abstract class Button : Label
    {
        /// <summary>
        /// Нажали ли на кнопку
        /// </summary>
        protected bool _isLeftDown;

        /// <summary>
        /// Сетка фона
        /// </summary>
        protected readonly MeshGuiColor _meshBg;

        public Button(WindowMain window, FontBase font, int width, string text, int height)
            : base(window, font, width, height, ChatStyle.Bolb + text + ChatStyle.Reset)
        {
            _meshBg = new MeshGuiColor(gl);
        }

        #region Draw

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected override void _RenderInside(RenderMain render, int x, int y)
            => base._RenderInside(render, x, y);

        /// <summary>
        /// Метод для прорисовки кадра
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public override void Draw(float timeIndex)
        {
            // Рисуем фон кнопки
            window.Render.BindTextureWidgets();
            _meshBg.Draw();
            // Рисуем текст кнопки
            base.Draw(timeIndex);
        }

        #endregion

        #region OnMouse

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
                        base._OnClick();
                    }
                }
                _isLeftDown = false;
                IsRender = true;
            }
        }

        #endregion

        protected override void _OnClick()
        {
            _isLeftDown = true;
            IsRender = true;
            // Звук клика
            window.SoundClick(.3f);
        }

        public override void Dispose()
        {
            base.Dispose();
            _meshBg?.Dispose();
        }
    }
}
