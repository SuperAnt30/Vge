using System;
using System.Runtime.CompilerServices;
using Vge.Games;
using Vge.Gui.Controls;
using Vge.Realms;
using Vge.Renderer.Font;
using Vge.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран настроек
    /// </summary>
    public class ScreenOptions : ScreenWindow
    {
        private readonly ScreenBase parent;
        /// <summary>
        /// Опции во время игры
        /// </summary>
        private readonly bool _inGame;
        /// <summary>
        /// Локальная ли игра
        /// </summary>
        private readonly bool _isGameLocal;

        protected readonly Label _labelSound;
        protected readonly Label _labelGraphics;
        protected readonly LineHorizontal _lineSound;
        protected readonly LineHorizontal _lineGraphics;
        protected readonly Label _labelNikname;
        protected readonly TextBox _textBoxNikname;
        protected readonly Slider _sliderFps;
        protected readonly Slider _sliderSoundVolume;
        protected readonly Slider _sliderMusicVolume;
        protected readonly Slider _sliderMouseSensitivity;
        protected readonly Slider _sliderOverviewChunk;
        protected readonly CheckBox _checkBoxBigInterface;
        protected readonly CheckBox _checkBoxVSinc;
        protected readonly CheckBox _checkBoxFullScreen;
        protected readonly CheckBox _checkBoxAmbientOcclusion;
        protected readonly CheckBox _checkBoxShadow;
        protected readonly Button _buttonNet;

        protected readonly Button _buttonDone;
        protected readonly Button _buttonCancel;

        public ScreenOptions(WindowMain window, ScreenBase parent, bool inGame) 
            : base(window, 512f, 512, 416, true)
        {
            _inGame = inGame;
            this.parent = parent;

            _isGameLocal = inGame && window.Game != null && window.Game.IsLoacl;
            
            FontBase font = window.Render.FontMain;

            _labelSound = new Label(window, font, 400, 16, ChatStyle.Bolb + L.T("Sound"));
            _labelSound.SetTextAlight(EnumAlight.Left, EnumAlightVert.Top);
            _labelGraphics = new Label(window, font, 400, 16, ChatStyle.Bolb + L.T("Graphics"));
            _labelGraphics.SetTextAlight(EnumAlight.Left, EnumAlightVert.Top);
            _lineSound = new LineHorizontal(window, 396);
            _lineGraphics = new LineHorizontal(window, 396);

            _buttonDone = new ButtonThin(window, font, 128, L.T("Done"));
            _buttonDone.Click += ButtonDone_Click;
            _buttonCancel = new ButtonThin(window, font, 128, L.T("Cancel"));
            _buttonCancel.Click += ButtonCancel_Click;

            _labelNikname = new Label(window, font, 128, 24, L.T("Nikname"));
            _labelNikname.SetTextAlight(EnumAlight.Left, EnumAlightVert.Middle);
            _textBoxNikname = new TextBox(window, font, 128, Options.Nickname, TextBox.EnumRestrictions.Name, 16);
            _textBoxNikname.SetEnable(!inGame);

            _sliderFps = new Slider(window, font, 200, 10, 260, 10, L.T("Fps"));
            _sliderFps.SetValue(Options.Fps).AddParam(260, L.T("MaxFps"));

            _sliderSoundVolume = new Slider(window, font, 200, 0, 100, 1, L.T("SoundVolume"));
            _sliderSoundVolume.SetValue(Options.SoundVolume)
                .AddParam(0, L.T("SoundVolumeOff")).AddParam(100, L.T("SoundVolumeMax"));
            _sliderMusicVolume = new Slider(window, font, 200, 0, 100, 1, L.T("MusicVolume"));
            _sliderMusicVolume.SetValue(Options.MusicVolume)
                .AddParam(0, L.T("MusicVolumeOff")).AddParam(100, L.T("MusicVolumeMax"));
            _sliderMusicVolume.SetEnable(false);

            _sliderMouseSensitivity = new Slider(window, font, 200, 0, 100, 1, L.T("MouseSensitivity"));
            _sliderMouseSensitivity.SetValue(Options.MouseSensitivity)
                .AddParam(0, L.T("SensitivityMin")).AddParam(100, L.T("SensitivityMax"));

            _sliderOverviewChunk = new Slider(window, font, 200, 2, 64, 1, L.T("OverviewChunk"));
            _sliderOverviewChunk.SetValue(Options.OverviewChunk);

            _checkBoxBigInterface = new CheckBox(window, font, 200, L.T("BigInterface"));
            _checkBoxBigInterface.SetChecked(Options.SizeInterface != 1);
            _checkBoxVSinc = new CheckBox(window, font, 200, L.T("VSync"));
            _checkBoxVSinc.SetChecked(Options.VSync);
            _checkBoxFullScreen = new CheckBox(window, font, 200, L.T("FullScreenReset"));
            _checkBoxFullScreen.SetChecked(Options.FullScreen);
            _checkBoxAmbientOcclusion = new CheckBox(window, font, 200, L.T("AmbientOcclusion"));
            _checkBoxAmbientOcclusion.SetChecked(Options.AmbientOcclusion);
            _checkBoxShadow = new CheckBox(window, font, 200, L.T("Shadow"));
            _checkBoxShadow.SetChecked(Options.Shadow);

            if (_isGameLocal && window.Game != null && window.Game is GameLocal gameLocal
                && gameLocal.IsRunNet())
            {
                _buttonNet = new ButtonThin(window, font, 128, L.T("NetOn"));
                _buttonNet.SetEnable(false);
            }
            else
            {
                _buttonNet = new ButtonThin(window, font, 128, L.T("Net"));
            }
            _buttonNet.Click += ButtonNet_Click;

            if (!_isGameLocal) _buttonNet.SetVisible(false);
        }

        /// <summary>
        /// Название заголовка
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override string _GetTitle() => L.T("Options");

        /// <summary>
        /// Закрытие скрина
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _Close() => window.LScreen.Parent(parent, EnumScreenParent.None);

        /// <summary>
        /// Клик за пределами окна
        /// </summary>
        protected override void _OnClickOutsideWindow() { }

        #region Clicks

        private void ButtonNet_Click(object sender, EventArgs e)
        {
            if (window.Game != null && window.Game is GameLocal gameLocal)
            {
                gameLocal.OpenNet(32021);
                _buttonNet.SetText(L.T("NetOn")).SetEnable(false);
            }
        }

        private void ButtonDone_Click(object sender, EventArgs e)
        {
            // Сохраняем настроки
            SaveOptions();
            window.LScreen.Parent(parent, EnumScreenParent.Yes);
        }

        private void ButtonCancel_Click(object sender, EventArgs e) => _Close();

        #endregion

        /// <summary>
        /// Сохранить опции, вернуть true если менялся FullScreen
        /// </summary>
        protected virtual void SaveOptions()
        {
            bool isFullScreen = Options.FullScreen != _checkBoxFullScreen.Checked;
            int si = _checkBoxBigInterface.Checked ? 2 : 1;
            bool isSizeInterface = Options.SizeInterface != si;
            bool isOverviewChunk = Options.OverviewChunk != _sliderOverviewChunk.Value;
            bool isAmbientOcclusion = Options.AmbientOcclusion != _checkBoxAmbientOcclusion.Checked;
            bool isShadow = Options.Shadow != _checkBoxShadow.Checked;
            if (Options.VSync != _checkBoxVSinc.Checked)
            {
                window.SetVSync(_checkBoxVSinc.Checked);
            }
            if (_inGame && Options.Fps != _sliderFps.Value)
            {
                window.SetWishFrame(_sliderFps.Value);
            }
            Options.Fps = _sliderFps.Value;
            Options.SoundVolume = _sliderSoundVolume.Value;
            Options.MusicVolume = _sliderMusicVolume.Value;
            Options.MouseSensitivity = _sliderMouseSensitivity.Value;
            Options.SizeInterface = si;
            Options.VSync = _checkBoxVSinc.Checked;
            Options.FullScreen = _checkBoxFullScreen.Checked;
            Options.Nickname = _textBoxNikname.Text;
            Options.OverviewChunk = (byte)_sliderOverviewChunk.Value;
            Options.AmbientOcclusion = _checkBoxAmbientOcclusion.Checked;
            Options.Shadow = _checkBoxShadow.Checked;
            window.OptionsSave();
            if (isFullScreen)
            {
                window.SetFullScreen(Options.FullScreen);
            }
            else if (isSizeInterface)
            {
                window.UpdateSizeInterface();
            }
            if (window.Game != null)
            {
                if (isOverviewChunk)
                {
                    window.Game.Player.SetOverviewChunk(Options.OverviewChunk, false);
                }
            }
            if (isAmbientOcclusion)
            {
                Gi.BlockRendFull.InitAmbientOcclusion();
                Gi.BlockAlphaRendFull.InitAmbientOcclusion();
                Gi.BlockLiquidAlphaRendFull.InitAmbientOcclusion();

                if (window.Game != null)
                {
                    window.Game.Player.RerenderAllChunks();
                }
            }
            if (isShadow && window.Game != null)
            {
                window.Game.WorldRender.ModifyShadow();
            }
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(_lineSound);
            AddControls(_lineGraphics);
            AddControls(_labelSound);
            AddControls(_labelGraphics);

            AddControls(_buttonDone);
            AddControls(_buttonCancel);

            AddControls(_labelNikname);
            AddControls(_textBoxNikname);
            AddControls(_sliderFps);
            AddControls(_sliderSoundVolume);
            AddControls(_sliderMusicVolume);
            AddControls(_sliderMouseSensitivity);
            AddControls(_sliderOverviewChunk);

            AddControls(_checkBoxBigInterface);
            AddControls(_checkBoxVSinc);
            AddControls(_checkBoxAmbientOcclusion);
            AddControls(_checkBoxShadow);

            AddControls(_buttonNet);
            AddControls(_checkBoxFullScreen);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            // Расположение окна
            PosX = (Width - WidthWindow) / 2;
            PosY = (Height - HeightWindow) / 2;
            base.OnResized();

            _labelNikname.SetPosition(PosX + 274, PosY + 32);
            _textBoxNikname.SetPosition(PosX + 348, PosY + 32);
            _sliderMouseSensitivity.SetPosition(PosX + 36, PosY + 62);
            _sliderOverviewChunk.SetPosition(PosX + 274, PosY + 62);

            // --Sound
            _lineSound.SetPosition(PosX + 80, PosY + 120);
            _labelSound.SetPosition(PosX + 14, PosY + 115);
            _sliderSoundVolume.SetPosition(PosX + 36, PosY + 140);
            _sliderMusicVolume.SetPosition(PosX + 274, PosY + 140);

            // --Graphics 
            _lineGraphics.SetPosition(PosX + 80, PosY + 200);
            _labelGraphics.SetPosition(PosX + 14, PosY + 195);
            _sliderFps.SetPosition(PosX + 36, PosY + 220);
            _checkBoxVSinc.SetPosition(PosX + 274, PosY + 232);
            _checkBoxFullScreen.SetPosition(PosX + 274, PosY + 262);
            _checkBoxBigInterface.SetPosition(PosX + 274, PosY + 292);
            
            _checkBoxAmbientOcclusion.SetPosition(PosX + 36, PosY + 262);
            _checkBoxShadow.SetPosition(PosX + 36, PosY + 292);

            _buttonNet.SetPosition(PosX + 192, PosY + 332);

            _buttonDone.SetPosition(PosX + 122, PosY + 372);
            _buttonCancel.SetPosition(PosX + 262, PosY + 372);
        }

        public override void Draw(float timeIndex)
        {
            if (!_inGame)
            {
                gl.ClearColor(.486f, .569f, .616f, 1f);
            }
            base.Draw(timeIndex);
        }
    }
}
