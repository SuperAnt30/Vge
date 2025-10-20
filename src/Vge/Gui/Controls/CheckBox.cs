using Vge.Renderer;
using Vge.Renderer.Font;
using WinGL.Actions;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол true / false
    /// </summary>
    public class CheckBox : Label
    {
        /// <summary>
        /// Смещение до текста, ширина калочки + шаг смещения
        /// </summary>
        private const int Prefix = 16 + 8;

        /// <summary>
        /// Значение
        /// </summary>
        public bool Checked { get; private set; } = false;

        /// <summary>
        /// Нажали ли на кнопку
        /// </summary>
        protected bool _isLeftDown;

        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;

        public CheckBox(WindowMain window, FontBase font, int width, string text)
            : base(window, font, width, 16, text)
        {
            _meshBg = new MeshGuiColor(gl);
            SetTextAlight(EnumAlight.Left, EnumAlightVert.Middle);
        }

        /// <summary>
        /// Задать значение
        /// </summary>
        public CheckBox SetChecked(bool check)
        {
            Checked = check;
            IsRender = true;
            return this;
        }

        #region Draw

        /// <summary>
        /// Прорисовка внутреняя
        /// </summary>
        /// <param name="x">Позиция X с учётом интерфейса</param>
        /// <param name="y">Позиция Y с учётом интерфейса</param>
        protected override void _RenderInside(RenderMain render, int x, int y)
        {
            // Рендер текста со смещением
            int w = Width;
            SetSize(Width - Prefix, Height);
            base._RenderInside(render, x + Prefix * _si, y);
            SetSize(w, Height);

            // Рендер Значка
            float u1 = Checked ? .0625f : 0;
            float v1 = Enabled ? _isLeftDown ? .75f : (Enter ? .6875f : .625f) : .5625f;

            _meshBg.Reload(RenderFigure.Rectangle(x, y, x + 32 * _si, y + 32 * _si,
                u1, v1, u1 + .0625f, v1 + .0625f));
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

        protected override void _OnClick()
        {
            _isLeftDown = true;
            IsRender = true;
            // Звук клика
            window.SoundClick(.3f);
        }

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
                        SetChecked(!Checked);
                        base._OnClick();
                    }
                }
                _isLeftDown = false;
                IsRender = true;
            }
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            _meshBg?.Dispose();
        }
    }
}
