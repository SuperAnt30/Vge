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
        public static string NameGameData(string path, int slot)
        {
            string pf = path + Path.DirectorySeparatorChar + NameFaileGame;
            if (File.Exists(pf))
            {
                // Дата последнего редактирования
                DateTime dateTime = File.GetLastWriteTime(pf);
                // Размер папки игры
                string size = new SizeDirectory(path).ToString();

                TagCompound nbt = NBTTools.ReadFromFile(pf, true);

                if (nbt.GetShort("Version") != Ce.IndexVersion)
                {
                    //Разная версия
                    return L.T("GameDifferentVersion");
                }
                // Определяем сколько времени играет
                long time = nbt.GetLong("TimeCounter");
                long m = time / 60000;
                long h = time / 3600000;
                m = m - h * 60;
                return string.Format("#{0} {1:dd.MM.yyyy} {2} " + L.T("Time") + " {3}:{4}", 
                    slot, dateTime, size,
                    h, m < 10 ? "0" + m : m.ToString()
                    );
            }
            // Сломан
            return L.T("GameBroken");
        }

        /// <summary>
        /// Проверка пути, если нет, то создаём
        /// </summary>
        public static void CheckPath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Сохранить игру
        /// </summary>
        public static void Write(GameSettings gameSettings, GameServer server)
        {
            try
            {
                string pathGame = gameSettings.PathGames;
                CheckPath(pathGame);

                TagCompound nbt = new TagCompound();
                nbt.SetLong("Seed", gameSettings.Seed);
                nbt.SetLong("TimeCounter", server.TimeCounter);
                nbt.SetShort("Version", Ce.IndexVersion);
                // Таблица блоков
                gameSettings.TableBlocks.Write("TableBlocks", nbt);
                // Таблица предметов
                gameSettings.TableItems.Write("TableItems", nbt);
                // Таблица сущностей
                gameSettings.TableEntities.Write("TableEntities", nbt);
                // Таблица блок сущностей
                gameSettings.TableBlocksEntity.Write("TableBlocksEntity", nbt);

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
