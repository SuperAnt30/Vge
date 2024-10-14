using System.IO;
using Vge.Util;

namespace Mvk2.Util
{
    public class OptionsFileMvk : OptionsFile
    {
        public OptionsFileMvk() => _title = "Mvk2 Project - (c)2024";

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
                    case "SmoothLighting": OptionsMvk.SmoothLighting = value == "1"; return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Загрузить построчно опции
        /// </summary>
        protected override void _SaveLine(StreamWriter file)
        {
            base._SaveLine(file);

            file.WriteLine("\r\n# Mvk");
            file.WriteLine("SmoothLighting: " + (OptionsMvk.SmoothLighting ? "1" : "0"));
        }
    }
}
