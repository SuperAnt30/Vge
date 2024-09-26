using System.IO;
using Vge.Util;

namespace Mvk2.Util
{
    public class OptionsFileMvk : OptionsFile
    {
        public OptionsFileMvk() => title = "Mvk2 Project - (c)2024";

        protected override void UpData()
        {
            base.UpData();
            OptionsMvk.UpDataMvk();
        }

        /// <summary>
        /// Загрузить настройки
        /// </summary>
        protected override bool ReadLine(string key, string value)
        {
            if (base.ReadLine(key, value))
            {
                return true;
            }
            else
            {
                switch (key)
                {
                    case "OverviewChunk": OptionsMvk.OverviewChunk = int.Parse(value); return true;
                    case "SmoothLighting": OptionsMvk.SmoothLighting = value == "1"; return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Загрузить построчно опции
        /// </summary>
        protected override void SaveLine(StreamWriter file)
        {
            base.SaveLine(file);

            file.WriteLine("\r\n# Mvk");
            file.WriteLine("OverviewChunk: " + OptionsMvk.OverviewChunk.ToString());
            file.WriteLine("SmoothLighting: " + (OptionsMvk.SmoothLighting ? "1" : "0"));
        }
    }
}
