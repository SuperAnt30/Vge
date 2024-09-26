﻿using Vge.Games;
using Vge.Gui.Controls;
using Vge.Realms;
using Vge.Renderer.Font;
using Vge.Util;

namespace Vge.Gui.Screens
{
    /// <summary>
    /// Экран настроек
    /// </summary>
    public class ScreenOptions : ScreenBase
    {
        private readonly ScreenBase parent;
        /// <summary>
        /// Опции во время игры
        /// </summary>
        private readonly bool inGame;
        /// <summary>
        /// Локальная ли игра
        /// </summary>
        private readonly bool isGameLocal;

        protected readonly Label label;
        protected readonly Slider sliderFps;
        protected readonly Slider sliderSoundVolume;
        protected readonly Slider sliderMouseSensitivity;
        protected readonly CheckBox checkBoxBigInterface;
        protected readonly CheckBox checkBoxVSinc;
        protected readonly CheckBox checkBoxFullScreen;
        protected readonly Button buttonNet;

        protected readonly Button buttonDone;
        protected readonly Button buttonCancel;

        public ScreenOptions(WindowMain window, ScreenBase parent, bool inGame) : base(window)
        {
            this.inGame = inGame;
            this.parent = parent;

            isGameLocal = inGame && window.Game != null && window.Game.IsLoacl;
            
            FontBase font = window.Render.FontMain;

            label = new Label(window, font, ChatStyle.Bolb + L.T("Options"));
            label.SetTextAlight(EnumAlight.Center, EnumAlightVert.Bottom);
            buttonDone = new Button(window, font, 300, L.T("Done"));
            buttonDone.Click += ButtonDone_Click;
            buttonCancel = new Button(window, font, 300, L.T("Cancel"));
            buttonCancel.Click += ButtonCancel_Click;

            sliderFps = new Slider(window, font, 300, 10, 260, 10, L.T("Fps"));
            sliderFps.SetValue(Options.Fps).AddParam(260, L.T("MaxFps"));

            sliderSoundVolume = new Slider(window, font, 300, 0, 100, 1, L.T("SoundVolume"));
            sliderSoundVolume.SetValue(Options.SoundVolume)
                .AddParam(0, L.T("VolumeOff")).AddParam(100, L.T("VolumeMax"));

            sliderMouseSensitivity = new Slider(window, font, 300, 0, 100, 1, L.T("MouseSensitivity"));
            sliderMouseSensitivity.SetValue(Options.MouseSensitivity)
                .AddParam(0, L.T("SensitivityMin")).AddParam(100, L.T("SensitivityMax"));

            checkBoxBigInterface = new CheckBox(window, font, 300, L.T("BigInterface"));
            checkBoxBigInterface.SetChecked(Options.SizeInterface != 1);
            checkBoxVSinc = new CheckBox(window, font, 300, L.T("VSync"));
            checkBoxVSinc.SetChecked(Options.VSync);
            checkBoxFullScreen = new CheckBox(window, font, 300, L.T("FullScreenReset"));
            checkBoxFullScreen.SetChecked(Options.FullScreen);

            buttonNet = new Button(window, font, 300, L.T("Net"));
            buttonNet.Click += ButtonNet_Click;
        }

        #region Clicks

        private void ButtonNet_Click(object sender, System.EventArgs e)
        {
            if (window.Game != null && window.Game is GameLocal gameLocal)
            {
                gameLocal.OpenNet();
                buttonNet.SetText(L.T("NetOn")).SetEnable(false);
            }
        }

        private void ButtonDone_Click(object sender, System.EventArgs e)
        {
            // Сохраняем настроки
            SaveOptions();
            window.LScreen.Parent(parent, EnumScreenParent.Yes);
        }

        private void ButtonCancel_Click(object sender, System.EventArgs e)
            => window.LScreen.Parent(parent, EnumScreenParent.None);

        #endregion

        /// <summary>
        /// Сохранить опции, вернуть true если менялся FullScreen
        /// </summary>
        protected virtual void SaveOptions()
        {
            bool isFullScreen = Options.FullScreen != checkBoxFullScreen.Checked;
            int si = checkBoxBigInterface.Checked ? 2 : 1;
            bool isSizeInterface = Options.SizeInterface != si;
            if (Options.VSync != checkBoxVSinc.Checked)
            {
                window.SetVSync(checkBoxVSinc.Checked);
            }
            if (Options.Fps != sliderFps.Value)
            {
                window.SetWishFrame(sliderFps.Value);
            }
            Options.Fps = sliderFps.Value;
            Options.SoundVolume = sliderSoundVolume.Value;
            Options.MouseSensitivity = sliderMouseSensitivity.Value;
            Options.SizeInterface = si;
            Options.VSync = checkBoxVSinc.Checked;
            Options.FullScreen = checkBoxFullScreen.Checked;
            window.OptionsSave();
            if (isFullScreen)
            {
                window.Restart();
            }
            else if (isSizeInterface)
            {
                window.UpdateSizeInterface();
            }
        }

        /// <summary>
        /// Запускается при создании объекта и при смене режима FullScreen
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            AddControls(label);
            AddControls(buttonDone);
            AddControls(buttonCancel);

            AddControls(sliderFps);
            AddControls(sliderSoundVolume);
            AddControls(sliderMouseSensitivity);

            AddControls(checkBoxBigInterface);
            AddControls(checkBoxVSinc);

            if (isGameLocal)
            {
                AddControls(buttonNet);
            }
            if (!inGame)
            {
                AddControls(checkBoxFullScreen);
            }
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int w = Width / 2;
            int h = Height / 2;
            label.SetSize(Width - 100, label.Height).SetPosition(50, h - label.Height - 200);

            buttonDone.SetPosition(w - buttonDone.Width - 2, h + 170);
            buttonCancel.SetPosition(w + 2, h + 170);

            sliderSoundVolume.SetPosition(w - sliderSoundVolume.Width / 2, h - 150);
            sliderFps.SetPosition(w - sliderFps.Width / 2, h - 100);
            sliderMouseSensitivity.SetPosition(w - sliderMouseSensitivity.Width / 2, h - 50);

            checkBoxBigInterface.SetPosition(w - checkBoxBigInterface.Width / 2, h);
            checkBoxVSinc.SetPosition(w - checkBoxVSinc.Width / 2, h + 50);
            checkBoxFullScreen.SetPosition(w - checkBoxFullScreen.Width / 2, h + 100);
            buttonNet.SetPosition(w - buttonNet.Width / 2, h + 100);
        }

        public override void Draw(float timeIndex)
        {
            if (!inGame)
            {
                gl.ClearColor(.5f, .3f, .02f, 1f);
            }
            base.Draw(timeIndex);
        }
    }
}
