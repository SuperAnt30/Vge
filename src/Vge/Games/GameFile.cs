using System;
using System.IO;
using Vge.NBT;
using Vge.Util;

namespace Vge.Games
{
    /// <summary>
    /// Объект работы сохранения игры в файл
    /// </summary>
    public class GameFile
    {
        /// <summary>
        /// Имя файла информации о игре
        /// </summary>
        public const string NameFaileGame = "level.dat";

        /// <summary>
        /// Получить название сохранённой игры
        /// </summary>
        public static string NameGameData(string path)
        {
            string pf = path + Path.DirectorySeparatorChar + NameFaileGame;
            if (File.Exists(pf))
            {
                TagCompound nbt = NBTTools.ReadFromFile(pf, true);

                if (nbt.GetShort("Version") != Ce.IndexVersion)
                {
                    //Разная версия
                    return L.T("GameDifferentVersion");
                }
                return nbt.GetString("LevelName");
            }
            // Сломан
            return L.T("GameBroken");
        }

        /// <summary>
        /// Проверка пути, если нет, то создаём
        /// </summary>
        private static void CheckPath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Сохранить игру
        /// </summary>
        public static void Write(GameSettings gameSettings, Server server)
        {
            try
            {
                string pathGame = gameSettings.GetPathGame();
                CheckPath(Options.PathGames);
                CheckPath(pathGame);

                TagCompound nbt = new TagCompound();
                nbt.SetLong("Seed", gameSettings.Seed);
                nbt.SetLong("TickCounter", server.TickCounter);
                nbt.SetLong("TimeCounter", server.TimeCounter);
                nbt.SetShort("Version", Ce.IndexVersion);

                
                NBTTools.WriteToFile(nbt, pathGame + NameFaileGame, true);
                server.Log.Server(Srl.ServerSavingGame);
                // Сохраняем чанки в регионы 
                //World.ChunkPrServ.SaveChunks();
                // Сохраняем регионы в файл
                //World.Regions.WriteToFile(true);
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
            }
        }

        /// <summary>
        /// Загрузить файл игры
        /// </summary>
        public static TagCompound ReadGame(string path)
        {
            string pathFile = path + NameFaileGame;
            if (File.Exists(pathFile))
            {
                return NBTTools.ReadFromFile(pathFile, true);
            }
            return null;
        }
    }
}
