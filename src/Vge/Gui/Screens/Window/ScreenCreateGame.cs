using System;
using System.Runtime.CompilerServices;
using Vge.Games;
using Vge.Gui.Controls;
using Vge.Renderer.Font;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран создания игры
    /// </summary>
    public class ScreenCreateGame : ScreenWindow
    {
        private readonly int _slot;
        protected readonly Label _labelSeed;
        protected readonly TextBox _textBoxSeed;
        protected readonly ButtonThin _buttonCreate;
        protected readonly ButtonThin _buttonCancel;

        public ScreenCreateGame(WindowMain window, int slot) : base(window, 512f, 320, 200, true)
        {
            _slot = slot;
            FontBase font = window.Render.FontMain;

            _labelSeed = new Label(window, font, 128, 24, L.T("Seed"));
            _labelSeed.SetTextAlight(EnumAlight.Right, EnumAlightVert.Middle);
            _textBoxSeed = new TextBox(window, font, 128, "", TextBox.EnumRestrictions.Number);

            _buttonCreate = new ButtonThin(window, font, 128, L.T("Create"));
            _buttonCreate.Click += ButtonCreate_Click;
            _buttonCancel = new ButtonThin(window, font, 128, L.T("Cancel"));
            _buttonCancel.Click += ButtonCancel_Click;
        }

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string _GetTitle() => L.T("SingleCreate{0}", _slot);

        /// <summary>
        /// Закрытие скрина
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _Close() => window.LScreen.Single();

        /// <summary>
        /// Клик за пределами окна
        /// </summary>
        protected override void _OnClickOutsideWindow() { }

        private void ButtonCreate_Click(object sender, EventArgs e)
        {
            // Создаём игру
            long seed;
            try
            {
                seed = _textBoxSeed.Text != "" ? long.Parse(_textBoxSeed.Text) : 0;
            }
            catch
            {
                seed = 0;
            }
            window.GameLocalRun(new GameSettings(_slot, seed));
        }

        private void ButtonCancel_Click(object sender, EventArgs e) => _Close();

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void _OnInitialize()
        {
            base._OnInitialize();
            _AddControls(_labelSeed);
            _AddControls(_textBoxSeed);
            _AddControls(_buttonCreate);
            _AddControls(_buttonCancel);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void _OnResized()
        {
            PosX = (Width - WidthWindow) / 2;
            PosY = (Height - HeightWindow) / 2;
            base._OnResized();

            _labelSeed.SetPosition(PosX + 26, PosY + 66);
            _textBoxSeed.SetPosition(PosX + 166, PosY + 66);

            _buttonCreate.SetPosition(PosX + 26, PosY + 156);
            _buttonCancel.SetPosition(PosX + 166, PosY + 156);
        }

        public override void Draw(float timeIndex)
        {
            gl.ClearColor(.486f, .569f, .616f, 1f);
            base.Draw(timeIndex);
        }
    }
}