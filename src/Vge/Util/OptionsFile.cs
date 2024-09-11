using System;
using System.IO;

namespace Vge.Util
{
    public class OptionsFile
    {
        /// <summary>
        /// Заголовок файла
        /// </summary>
        protected string title = "Vge Project - (c)2024";
        /// <summary>
        /// Имя файла настроек по умолчанию "options.ini"
        /// </summary>
        protected string fileName = "options.ini";

        /// <summary>
        /// Загрузить настройки, true вернёт если файл был, false если файла настроек нет
        /// </summary>
        public void Load()
        {
            if (File.Exists(fileName))
            {
                //получить доступ к  существующему либо создать новый
                using (StreamReader file = new StreamReader(fileName))
                {
                    while (true)
                    {
                        // Читаем строку из файла во временную переменную.
                        string strLine = file.ReadLine();

                        // Если достигнут конец файла, прерываем считывание.
                        if (strLine == null) break;

                        // комментарий
                        if (strLine.Length == 0 || strLine.Substring(0, 1) == "#") continue;

                        string[] vs = strLine.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                        if (vs.Length == 2) ReadLine(vs[0], vs[1]);
                    }
                }
                UpData();
            }
            else
            {
                // Если файла опций нет, мы его создаём
                Save();
            }
        }

        /// <summary>
        /// Сохранить настройки
        /// </summary>
        public void Save()
        {
            using (StreamWriter file = new StreamWriter(fileName))
            {
                file.WriteLine("# " + title);
                file.WriteLine("# File Created: {0:dd.MM.yyyy HH:mm.ss}" + Ce.Br, DateTime.Now);
                SaveLine(file);
                file.Close();
            }
            UpData();
        }

        protected virtual void UpData() => Options.UpData();

        /// <summary>
        /// Загрузить настройки
        /// </summary>
        protected virtual bool ReadLine(string key, string value)
        {
            switch(key)
            {
                case "PathAssets": Options.PathAssets = value; return true;
                case "SizeInterface": Options.SizeInterface = int.Parse(value); return true;
                case "SoundVolume": Options.SoundVolume = int.Parse(value); return true;
                case "Fps": Options.Fps = int.Parse(value); return true;
                case "MouseSensitivity": Options.MouseSensitivity = int.Parse(value); return true;
            }
            return false;
        }

        /// <summary>
        /// Загрузить построчно опции
        /// </summary>
        protected virtual void SaveLine(StreamWriter file)
        {
            file.WriteLine("PathAssets: " + Options.PathAssets);
            file.WriteLine("SizeInterface: " + Options.SizeInterface.ToString());
            file.WriteLine("SoundVolume: " + Options.SoundVolume.ToString());
            file.WriteLine("Fps: " + Options.Fps.ToString());
            file.WriteLine("MouseSensitivity: " + Options.MouseSensitivity.ToString());
        }
    }
}
