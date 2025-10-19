﻿using System;
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
        protected readonly TextBox textBoxNikame;
        protected readonly Slider sliderFps;
        protected readonly Slider sliderSoundVolume;
        protected readonly Slider sliderMusicVolume;
        protected readonly Slider sliderMouseSensitivity;
        protected readonly Slider sliderOverviewChunk;
        protected readonly CheckBox checkBoxBigInterface;
        protected readonly CheckBox checkBoxVSinc;
        protected readonly CheckBox checkBoxFullScreen;
        protected readonly CheckBox checkBoxQualitatively;
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
            buttonDone = new ButtonThin(window, font, 128, L.T("Done"));
            buttonDone.Click += ButtonDone_Click;
            buttonCancel = new ButtonThin(window, font, 128, L.T("Cancel"));
            buttonCancel.Click += ButtonCancel_Click;

            textBoxNikame = new TextBox(window, font, 300, Options.Nickname, TextBox.EnumRestrictions.Name, 16);
            textBoxNikame.SetEnable(!inGame);

            sliderFps = new Slider(window, font, 300, 10, 260, 10, L.T("Fps"));
            sliderFps.SetValue(Options.Fps).AddParam(260, L.T("MaxFps"));

            sliderSoundVolume = new Slider(window, font, 300, 0, 100, 1, L.T("SoundVolume"));
            sliderSoundVolume.SetValue(Options.SoundVolume)
                .AddParam(0, L.T("SoundVolumeOff")).AddParam(100, L.T("SoundVolumeMax"));
            sliderMusicVolume = new Slider(window, font, 300, 0, 100, 1, L.T("MusicVolume"));
            sliderMusicVolume.SetValue(Options.MusicVolume)
                .AddParam(0, L.T("MusicVolumeOff")).AddParam(100, L.T("MusicVolumeMax"));
            sliderMusicVolume.SetEnable(false);

            sliderMouseSensitivity = new Slider(window, font, 300, 0, 100, 1, L.T("MouseSensitivity"));
            sliderMouseSensitivity.SetValue(Options.MouseSensitivity)
                .AddParam(0, L.T("SensitivityMin")).AddParam(100, L.T("SensitivityMax"));

            sliderOverviewChunk = new Slider(window, font, 300, 2, 64, 1, L.T("OverviewChunk"));
            sliderOverviewChunk.SetValue(Options.OverviewChunk);

            checkBoxBigInterface = new CheckBox(window, font, 300, L.T("BigInterface"));
            checkBoxBigInterface.SetChecked(Options.SizeInterface != 1);
            checkBoxVSinc = new CheckBox(window, font, 300, L.T("VSync"));
            checkBoxVSinc.SetChecked(Options.VSync);
            checkBoxFullScreen = new CheckBox(window, font, 300, L.T("FullScreenReset"));
            checkBoxFullScreen.SetChecked(Options.FullScreen);
            checkBoxQualitatively = new CheckBox(window, font, 300, L.T("Qualitatively"));
            checkBoxQualitatively.SetChecked(Options.Qualitatively);
            checkBoxQualitatively.SetEnable(false);

            if (isGameLocal && window.Game != null && window.Game is GameLocal gameLocal
                && gameLocal.IsRunNet())
            {
                buttonNet = new ButtonThin(window, font, 128, L.T("NetOn"));
                buttonNet.SetEnable(false);
            }
            else
            {
                buttonNet = new ButtonThin(window, font, 128, L.T("Net"));
            }
            buttonNet.Click += ButtonNet_Click;

            if (!isGameLocal) buttonNet.SetVisible(false);
           // if (inGame) checkBoxFullScreen.SetVisible(false);
        }

        #region Clicks

        private void ButtonNet_Click(object sender, EventArgs e)
        {
            if (window.Game != null && window.Game is GameLocal gameLocal)
            {
                gameLocal.OpenNet(32021);
                buttonNet.SetText(L.T("NetOn")).SetEnable(false);
            }
        }

        private void ButtonDone_Click(object sender, EventArgs e)
        {
            // Сохраняем настроки
            SaveOptions();
            window.LScreen.Parent(parent, EnumScreenParent.Yes);
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
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
            bool isOverviewChunk = Options.OverviewChunk != sliderOverviewChunk.Value;
            bool isQualitatively = Options.Qualitatively != checkBoxQualitatively.Checked;
            if (Options.VSync != checkBoxVSinc.Checked)
            {
                window.SetVSync(checkBoxVSinc.Checked);
            }
            if (inGame && Options.Fps != sliderFps.Value)
            {
                window.SetWishFrame(sliderFps.Value);
            }
            Options.Fps = sliderFps.Value;
            Options.SoundVolume = sliderSoundVolume.Value;
            Options.MusicVolume = sliderMusicVolume.Value;
            Options.MouseSensitivity = sliderMouseSensitivity.Value;
            Options.SizeInterface = si;
            Options.VSync = checkBoxVSinc.Checked;
            Options.FullScreen = checkBoxFullScreen.Checked;
            Options.Nickname = textBoxNikame.Text;
            Options.OverviewChunk = (byte)sliderOverviewChunk.Value;
            Options.Qualitatively = checkBoxQualitatively.Checked;
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
            if (isQualitatively)
            {
                Gi.BlockRendFull.InitAmbientOcclusion();
                Gi.BlockAlphaRendFull.InitAmbientOcclusion();
                Gi.BlockLiquidAlphaRendFull.InitAmbientOcclusion();

                if (window.Game != null)
                {
                    window.Game.Player.RerenderAllChunks();
                    window.Game.WorldRender.ModifyQualitatively();
                }
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

            AddControls(textBoxNikame);
            AddControls(sliderFps);
            AddControls(sliderSoundVolume);
            AddControls(sliderMusicVolume);
            AddControls(sliderMouseSensitivity);
            AddControls(sliderOverviewChunk);

            AddControls(checkBoxBigInterface);
            AddControls(checkBoxVSinc);
            AddControls(checkBoxQualitatively);

            AddControls(buttonNet);
            AddControls(checkBoxFullScreen);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void OnResized()
        {
            int w = Width / 2;
            int h = Height / 2;
            label.SetSize(Width - 100, label.Height).SetPosition(50, h - label.Height - 220);

            textBoxNikame.SetPosition(w - textBoxNikame.Width / 2, h - 200);
            sliderSoundVolume.SetPosition(w - sliderSoundVolume.Width - 2, h - 156);
            sliderMusicVolume.SetPosition(w + 2, h - 156);//112
            sliderMouseSensitivity.SetPosition(w - sliderMouseSensitivity.Width - 2, h - 112);
            sliderOverviewChunk.SetPosition(w + 2, h - 112);
            sliderFps.SetPosition(w - sliderFps.Width - 2, h - 68);
            checkBoxFullScreen.SetPosition(w + 2, h - 68);
            checkBoxBigInterface.SetPosition(w - checkBoxBigInterface.Width - 2, h - 24);
            checkBoxVSinc.SetPosition(w + 2, h - 24);
            checkBoxQualitatively.SetPosition(w - checkBoxQualitatively.Width, h + 20);

            //-24
            // +20
            //+64 +108



            buttonNet.SetPosition(w - buttonNet.Width / 2, h + 108);

            buttonDone.SetPosition(w - buttonDone.Width - 2, h + 180);
            buttonCancel.SetPosition(w + 2, h + 180);
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
