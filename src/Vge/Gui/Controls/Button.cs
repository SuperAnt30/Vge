using Vge.Renderer;
using Vge.Renderer.Font;
using WinGL.Actions;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол кнопки
    /// </summary>
    public class Button : Label
    {
        /// <summary>
        /// Нажали ли на кнопку
        /// </summary>
        private bool _click;

        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;

        public Button(WindowMain window, FontBase font, int width, string text, int height = 40)
            : base(window, font, width, height, text)
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
        {
            base._RenderInside(render, x, y);
            float v1 = Enabled ? Enter ? vk + vk : vk : 0f;
            if (_click) v1 = 0; // Временно!
            _meshBg.Reload(_RectangleTwo(x, y, 0, v1, vk, 1, 1, 1));
        }

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
            if (_click)
            {
                if (button == MouseButton.Left)
                {
                    OnMouseMove(x, y);
                    if (Enter)
                    {
                        base._OnClick();
                    }
                }
                _click = false;
                IsRender = true;
            }
        }

        #endregion

        protected override void _OnClick()
        {
            _click = true;
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
