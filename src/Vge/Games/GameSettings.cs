using System;
using System.IO;
using Vge.NBT;
using Vge.Util;

namespace Vge.Games
{
    /// <summary>
    /// Игровой объект настроек игры
    /// </summary>
    public class GameSettings
    {
        /// <summary>
        /// Игровое зерно, для разнообразия игры
        /// </summary>
        public long Seed { get; private set; }
        /// <summary>
        /// Номер слота, где выбрана игра, Навзание папки
        /// </summary>
        public byte Slot { get; private set; }
        /// <summary>
        /// Название папки сохранённой игры
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// Корректировочная таблица Id блоков
        /// </summary>
        public readonly CorrectTable TableBlocks = new CorrectTable();
        /// <summary>
        /// Корректировочная таблица Id предметов
        /// </summary>
        public readonly CorrectTable TableItems = new CorrectTable();
        /// <summary>
        /// Корректировочная таблица Id сущностей
        /// </summary>
        public readonly CorrectTable TableEntities = new CorrectTable();

        #region PathFile

        /// <summary>
        /// Путь к сохранении игр
        /// </summary>
        public readonly string PathGames;
        /// <summary>
        /// Путь к сохранении игроков в игре
        /// </summary>
        public readonly string PathPlayers;
        /// <summary>
        /// Путь к сохранении миров в игре
        /// </summary>
        private readonly string _pathWorld;

        #endregion

        private readonly bool _isLoad;

        /// <summary>
        /// Получить настройки по загрузке мира
        /// </summary>
        public GameSettings(int slot)
        {
            _isLoad = true;
            Slot = (byte)slot;
            FileName = (slot + 1).ToString();

            PathGames = Options.PathGames + FileName + Path.DirectorySeparatorChar;
            PathPlayers = PathGames + "Players" + Path.DirectorySeparatorChar;
            _pathWorld = PathGames + "World";
        }

        /// <summary>
        /// Получить путь к папке мира по его id
        /// </summary>
        public string GetPathWorld(byte idWorld)
        {
            idWorld++;
            return _pathWorld + (idWorld > 1 ? idWorld.ToString() : "") + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Создать мир
        /// </summary>
        public GameSettings(int slot, long seed) : this(slot)
        {
            _isLoad = false;
            if (seed == 0)
            {
                // Генерация нового сида
                seed = DateTime.Now.Ticks;
            }
            Seed = seed;
        }

        public bool Init(GameServer server)
        {
            if (_isLoad)
            {
                TagCompound nbt = GameFile.ReadGame(PathGames);
                if (nbt == null)
                {
                    return false;
                }
                server.SetDataFile(nbt.GetLong("TimeCounter"), (uint)nbt.GetLong("TickCounter"));
                Seed = nbt.GetLong("Seed");
                TableBlocks.Read("TableBlocks", nbt);
                TableItems.Read("TableItems", nbt);
                TableEntities.Read("TableEntities", nbt);
            }
            return true;
        }
    }
}
