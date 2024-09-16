using System.IO;
using Vge.Util;

namespace Vge.Games
{
    /// <summary>
    /// Объект списков игр для одиночной игры
    /// </summary>
    public class ListSingleGame
    {
        /// <summary>
        /// Количество слотов игр
        /// </summary>
        public const int CountSlot = 8;

        /// <summary>
        /// Список названий игр
        /// </summary>
        public readonly string[] NameWorlds = new string[CountSlot];
        /// <summary>
        /// Пустые игры
        /// </summary>
        public readonly bool[] EmptyWorlds = new bool[CountSlot];

        protected WindowMain window;


       // private WorldFile worldFile;


        public ListSingleGame(WindowMain window)
        {
            this.window = window;
            //worldFile = new WorldFile();
        }

        /// <summary>
        /// Загрузка миров
        /// </summary>
        public void Initialize()
        {
            for (int i = 0; i < CountSlot; i++)
            {
                if (Directory.Exists(Options.PathGames + (i + 1).ToString()))
                {
                    NameWorlds[i] = "Мир типа есть";// worldFile.NameWorldData(path);
                    EmptyWorlds[i] = false;
                }
                else
                {
                    NameWorlds[i] = "Мир пустой";
                    EmptyWorlds[i] = true;
                }
            }
        }

        /// <summary>
        /// Удалить игровой слот
        /// </summary>
        public void GameRemove(int slot)
        {
            DeleteDirectory(Options.PathGames + (slot + 1).ToString());
            EmptyWorlds[slot] = true;
            NameWorlds[slot] = "Kick";
        }


        /// <summary>
        /// Удалить папку с файлами
        /// Directory.Delete(path, true); // выплёвывает исключение
        /// </summary>
        private void DeleteDirectory(string path)
        {
            // https://overcoder.net/q/11892/%D0%BD%D0%B5%D0%B2%D0%BE%D0%B7%D0%BC%D0%BE%D0%B6%D0%BD%D0%BE-%D1%83%D0%B4%D0%B0%D0%BB%D0%B8%D1%82%D1%8C-%D0%BA%D0%B0%D1%82%D0%B0%D0%BB%D0%BE%D0%B3-%D1%81-%D0%BF%D0%BE%D0%BC%D0%BE%D1%89%D1%8C%D1%8E-directorydelete-%D0%BF%D1%83%D1%82%D1%8C-%D0%B8%D1%81%D1%82%D0%B8%D0%BD%D0%B0
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                string[] dirs = Directory.GetDirectories(path);

                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                foreach (string dir in dirs)
                {
                    DeleteDirectory(dir);
                }

                bool b = true;
                while (b)
                {
                    try
                    {
                        Directory.Delete(path, false);
                        b = false;
                    }
                    catch { }
                }
            }
        }
        
    }
}
