using Vge.Renderer;
using Vge.Renderer.Font;

namespace Vge.Gui.Controls
{
    /// <summary>
    /// Контрол true / false
    /// </summary>
    public class CheckBox : Label
    {
        /// <summary>
        /// Ширина Box
        /// </summary>
        private const int BoxWidth = 40;
        /// <summary>
        /// Смещение до текста
        /// </summary>
        private const int Prefix = BoxWidth + 8;
        /// <summary>
        /// Вертикаль к текстуре 200 / 512
        /// </summary>
        private const float Ver1 = .390625f + .5f;
        /// <summary>
        /// Вертикаль к текстуре 240 / 512
        /// </summary>
        private const float Ver2 = .46875f + .5f;
        /// <summary>
        /// Смещение при Checked 120 / 512
        /// </summary>
        private const float HorStepCheck = .234375f;

        /// <summary>
        /// Значение
        /// </summary>
        public bool Checked { get; private set; } = false;

        /// <summary>
        /// Сетка фона
        /// </summary>
        private readonly MeshGuiColor _meshBg;

        public CheckBox(WindowMain window, FontBase font, int width, string text)
            : base(window, font, width, 40, text)
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
            float u1 = Enabled ? Enter ? vk + vk : vk : 0f;
            if (Checked) u1 += HorStepCheck;

            _meshBg.Reload(RenderFigure.Rectangle(x, y, x + BoxWidth * _si, y + Height * _si,
                u1, Ver1, u1 + vk, Ver2));
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
            // Звук клика
            window.SoundClick(.3f);
            SetChecked(!Checked);
            base._OnClick();
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_meshBg != null) _meshBg.Dispose();
        }
    }
}
