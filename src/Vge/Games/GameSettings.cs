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

        private readonly bool isLoad;

        /// <summary>
        /// Получить настройки по загрузке мира
        /// </summary>
        public GameSettings(int slot)
        {
            isLoad = true;
            Slot = (byte)slot;
            FileName = (slot + 1).ToString();
        }

        /// <summary>
        /// Создать мир
        /// </summary>
        public GameSettings(int slot, long seed) : this(slot)
        {
            isLoad = false;
            if (seed == 0)
            {
                // Генерация нового сида
                seed = DateTime.Now.Ticks;
            }
            Seed = seed;
        }

        public bool Init(Server server)
        {
            if (isLoad)
            {
                TagCompound nbt = GameFile.ReadGame(GetPathGame());
                if (nbt == null)
                {
                    return false;
                }
                server.SetDataFile(nbt.GetLong("TimeCounter"), (uint)nbt.GetLong("TickCounter"));
                Seed = nbt.GetLong("Seed");
            }
            return true;
        }


        /// <summary>
        /// Получить адрес сохранённой игры
        /// </summary>
        public string GetPathGame() => Options.PathGames + FileName + Path.DirectorySeparatorChar;
    }
}
