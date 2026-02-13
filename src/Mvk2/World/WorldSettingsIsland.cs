using Mvk2.Item;
using Mvk2.World.BlockEntity.List;
using Mvk2.World.Gen;
using System.IO;
using Vge.Games;
using Vge.Item;
using Vge.NBT;
using Vge.World;
using Vge.World.Сalendar;

namespace Mvk2.World
{
    /// <summary>
    /// Опции основного мира остров
    /// </summary>
    public class WorldSettingsIsland : WorldSettings
    {
        /// <summary>
        /// Дыра хранилища на весь мир
        /// </summary>
        public readonly BlockHole StorageHole;

        /// <summary>
        /// Для клиента
        /// </summary>
        public WorldSettingsIsland() : base() { }

        /// <summary>
        /// Для сервера
        /// </summary>
        public WorldSettingsIsland(string pathWorld, GameServer server) : base(pathWorld)
        {
            // Сид используется только в серверной части
            ChunkGenerate = new ChunkProviderGenerateIsland(NumberChunkSections, server.Settings.Seed);

            StorageHole = new BlockHole(server);

            if (File.Exists(_pathFileSetting))
            {
                TagCompound nbt = NBTTools.ReadFromFile(_pathFileSetting, true);
                // Загрузить дыру хранилищ
                Calendar.SetTickCounter((uint)nbt.GetLong("TickCounter"));
                StorageHole.ReadFromNBT(nbt);
            }
            else
            {
                // Если нет файла, заполнить дефаултными
                StorageHole.SetStackInSlot(0, new ItemStack(ItemsRegMvk.AxeIron, 1, 315));
                StorageHole.SetStackInSlot(1, new ItemStack(ItemsRegMvk.Tie));
                StorageHole.SetStackInSlot(2, new ItemStack(ItemsRegMvk.ShirtBranded));
                StorageHole.SetStackInSlot(3, new ItemStack(ItemsRegMvk.BootsBranded));
                StorageHole.SetStackInSlot(4, new ItemStack(ItemsRegMvk.BackpackBranded));
                StorageHole.SetStackInSlot(5, new ItemStack(ItemsRegMvk.CapDark));

                StorageHole.SetStackInSlot(17, new ItemStack(ItemsRegMvk.Cobblestone, 7));
                StorageHole.SetStackInSlot(18, new ItemStack(ItemsRegMvk.Brol, 2));
            }
        }

        protected override void _Init()
        {
            IdSetting = 1;
            ActiveRadius = 8;
            NumberChunkSections = 8;
            //Calendar = new Сalendar32(36000); // 24000 при 20 тиках = 20 мин. При 30 тиках = 36000
            Calendar = new Сalendar32(36000);
        }

        /// <summary>
        /// Сохранить данные мира
        /// </summary>
        protected override void _WriteToNBT(TagCompound nbt)
        {
            base._WriteToNBT(nbt);
            StorageHole.WriteToNBT(nbt);
        }
    }
}
