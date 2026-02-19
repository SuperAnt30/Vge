using System.IO;
using Vge.Util;

namespace Mvk2.Util
{
    public class OptionsFileMvk : OptionsFile
    {
        public OptionsFileMvk()
        {
            _title = "Mvk2 Project - (c)2024-2026";
            Options.PrefixPath = "Mvk2";
        }

        protected override void _UpData()
        {
            base._UpData();
            OptionsMvk.UpDataMvk();
        }

        /// <summary>
        /// Загрузить настройки
        /// </summary>
        protected override bool _ReadLine(string key, string value)
        {
            if (base._ReadLine(key, value))
            {
                return true;
            }
            else
            {
                switch (key)
                {
                    case "Control": OptionsMvk.ControlInventory = int.Parse(value); return true;
                        // case "SmoothLighting": OptionsMvk.SmoothLighting = value == "1"; return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Сохранить построчно опции
        /// </summary>
        protected override void _WriteLine(StreamWriter file)
        {
           // base._WriteLine(file);

            file.WriteLine("PathAssets: " + Options.PathAssets);
            file.WriteLine("PathGames: " + Options.PathGames);
            file.WriteLine("\r\n# General");
            file.WriteLine("Nickname: " + Options.Nickname);
            file.WriteLine("Token: " + Options.Token);
            file.WriteLine("IpAddress: " + Options.IpAddress.ToString());
            file.WriteLine("\r\n# Graphics");
            file.WriteLine("AmbientOcclusion: " + (Options.AmbientOcclusion ? "1" : "0"));
            file.WriteLine("Shadow: " + (Options.Shadow ? "1" : "0"));
            file.WriteLine("FullScreen: " + (Options.FullScreen ? "1" : "0"));
            file.WriteLine("VSync: " + (Options.VSync ? "1" : "0"));
            file.WriteLine("SizeInterface: " + Options.SizeInterface.ToString());
            file.WriteLine("Fps: " + Options.Fps.ToString());
            file.WriteLine("\r\n# Audio");
            file.WriteLine("SoundVolume: " + Options.SoundVolume.ToString());
            file.WriteLine("MusicVolume: " + Options.MusicVolume.ToString());
            file.WriteLine("\r\n# Controls");
            file.WriteLine("MouseSensitivity: " + Options.MouseSensitivity.ToString());
            file.WriteLine("ControlForward: " + Options.ControlForward.ToString());
            file.WriteLine("ControlStrafeLeft: " + Options.ControlStrafeLeft.ToString());
            file.WriteLine("ControlStrafeRight: " + Options.ControlStrafeRight.ToString());
            file.WriteLine("ControlBack: " + Options.ControlBack.ToString());
            file.WriteLine("ControlJump: " + Options.ControlJump.ToString());
            file.WriteLine("ControlSneak: " + Options.ControlSneak.ToString());
            file.WriteLine("ControlSprinting: " + Options.ControlSprinting.ToString());
            file.WriteLine("ControlHandAction: " + Options.ControlHandAction.ToString());
            file.WriteLine("ControlItemUse: " + Options.ControlItemUse.ToString());
            file.WriteLine("ControlInventory: " + OptionsMvk.ControlInventory.ToString());
            file.WriteLine("\r\n# Game");
            file.WriteLine("OverviewChunk: " + Options.OverviewChunk.ToString());
        }
    }
}
