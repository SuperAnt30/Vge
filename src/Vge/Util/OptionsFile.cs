using System;
using System.IO;

namespace Vge.Util
{
    public class OptionsFile
    {
        /// <summary>
        /// Заголовок файла
        /// </summary>
        protected string _title = "Vge Project - (c)2024";
        /// <summary>
        /// Имя файла настроек по умолчанию "options.ini"
        /// </summary>
        protected string _fileName = "options.ini";

        /// <summary>
        /// Загрузить настройки, true вернёт если файл был, false если файла настроек нет
        /// </summary>
        public void Load()
        {
            if (File.Exists(_fileName))
            {
                // Получить доступ к существующему либо создать новый
                using (StreamReader file = new StreamReader(_fileName))
                {
                    while (true)
                    {
                        // Читаем строку из файла во временную переменную.
                        string strLine = file.ReadLine();

                        // Если достигнут конец файла, прерываем считывание.
                        if (strLine == null) break;

                        // комментарий
                        if (Sundry.ChekComment(strLine)) continue;

                        string[] vs = strLine.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);

                        if (vs.Length == 2) _ReadLine(vs[0], vs[1]);
                    }
                }
                _UpData();
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
            using (StreamWriter file = new StreamWriter(_fileName))
            {
                file.WriteLine("# " + _title);
                file.WriteLine("# File Created: {0:dd.MM.yyyy HH:mm.ss}" + Ce.Br, DateTime.Now);
                _WriteLine(file);
                file.Close();
            }
            _UpData();
        }

        protected virtual void _UpData() => Options.UpData();

        /// <summary>
        /// Загрузить настройки
        /// </summary>
        protected virtual bool _ReadLine(string key, string value)
        {
            switch(key)
            {
                case "PathAssets": Options.PathAssets = value; return true;
                case "PathGames": Options.PathGames = value; return true;
                case "Nickname": Options.Nickname = value; return true;
                case "Token": Options.Token = value; return true;
                case "IpAddress": Options.IpAddress = value.ToString(); return true;
                case "FullScreen": Options.FullScreen = value == "1"; return true;
                case "VSync": Options.VSync = value == "1"; return true;
                case "SizeInterface": Options.SizeInterface = int.Parse(value); return true;
                case "AmbientOcclusion": Options.AmbientOcclusion = value == "1"; return true;
                case "Shadow": Options.Shadow = value == "1"; return true;
                case "Fps": Options.Fps = int.Parse(value); return true;
                case "Fov": Options.Fov = int.Parse(value); return true;
                case "SoundVolume": Options.SoundVolume = int.Parse(value); return true;
                case "MusicVolume": Options.MusicVolume = int.Parse(value); return true;
                case "MouseSensitivity": Options.MouseSensitivity = int.Parse(value); return true;
                case "OverviewChunk": Options.OverviewChunk = byte.Parse(value); return true;
                case "ControlForward": Options.ControlForward = int.Parse(value); return true;
                case "ControlStrafeLeft": Options.ControlStrafeLeft = int.Parse(value); return true;
                case "ControlStrafeRight": Options.ControlStrafeRight = int.Parse(value); return true;
                case "ControlBack": Options.ControlBack = int.Parse(value); return true;
                case "ControlJump": Options.ControlJump = int.Parse(value); return true;
                case "ControlSneak": Options.ControlSneak = int.Parse(value); return true;
                case "ControlSprinting": Options.ControlSprinting = int.Parse(value); return true;
                case "ControlHandAction": Options.ControlHandAction = int.Parse(value); return true;
                case "ControlHandSecond": Options.ControlHandSecond = int.Parse(value); return true;
            }
            return false;
        }

        /// <summary>
        /// Сохранить построчно опции
        /// </summary>
        protected virtual void _WriteLine(StreamWriter file)
        {
            //file.WriteLine("PathAssets: " + Options.PathAssets);
            //file.WriteLine("PathGames: " + Options.PathGames);
            //file.WriteLine("\r\n# General");
            //file.WriteLine("Nickname: " + Options.Nickname);
            //file.WriteLine("Token: " + Options.Token);
            //file.WriteLine("IpAddress: " + Options.IpAddress.ToString());
            //file.WriteLine("\r\n# Graphics");
            //file.WriteLine("AmbientOcclusion: " + (Options.AmbientOcclusion ? "1" : "0"));
            //file.WriteLine("Shadow: " + (Options.Shadow ? "1" : "0"));
            //file.WriteLine("FullScreen: " + (Options.FullScreen ? "1" : "0"));
            //file.WriteLine("VSync: " + (Options.VSync ? "1" : "0"));
            //file.WriteLine("SizeInterface: " + Options.SizeInterface.ToString());
            //file.WriteLine("Fps: " + Options.Fps.ToString());
            //file.WriteLine("\r\n# Audio");
            //file.WriteLine("SoundVolume: " + Options.SoundVolume.ToString());
            //file.WriteLine("MusicVolume: " + Options.MusicVolume.ToString());
            //file.WriteLine("\r\n# Controls");
            //file.WriteLine("MouseSensitivity: " + Options.MouseSensitivity.ToString());
            //file.WriteLine("\r\n# Game");
            //file.WriteLine("OverviewChunk: " + Options.OverviewChunk.ToString());
        }
    }
}
