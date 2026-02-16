using Mvk2.Entity.Inventory;
using Mvk2.Item;
using Mvk2.Packets;
using Mvk2.World;
using Mvk2.World.Block;
using System.Runtime.CompilerServices;
using Vge.Entity.Inventory;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Item;
using Vge.NBT;
using Vge.Network;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;
using Vge.World;
using Vge.World.Block;

namespace Mvk2.Entity.List
{
    /// <summary>
    /// Объект игрока, как локального так и сетевого, не сущность
    /// </summary>
    public class PlayerServerMvk : PlayerServer
    {
        /// <summary>
        /// Объект инвенторя игрока
        /// </summary>
        public InventoryPlayerMvk InvPlayer { get; private set; }

        public PlayerServerMvk(string login, string token, SocketSide socket, GameServer server) 
            : base(login, token, socket, server) { }

        /// <summary>
        /// Создать игрока, тут первый спавн игрока
        /// </summary>
        public override void CreatePlayer()
        {
            // TODO::2026-02-15 Именно тут надо определять где спавнится игрок впервые!
            PosPrevX = PosX = -320;
            PosPrevY = PosY = 100;
            PosPrevZ = PosZ = 130;
            // Карманы
            //Inventory.SetStackInSlot(0, new ItemStack(Ce.Items.ItemObjects[0], 1, 315));
            //Inventory.SetStackInSlot(1, new ItemStack(Ce.Items.ItemObjects[1], 12));
            //Inventory.SetStackInSlot(2, new ItemStack(Ce.Items.ItemObjects[2], 16));

            // Левая рука
            Inventory.SetStackInSlot(12, new ItemStack(ItemsRegMvk.AxeIron, 1, 200));

            // Одежда
            Inventory.SetStackInSlot(13, new ItemStack(ItemsRegMvk.CapDark));
            Inventory.SetStackInSlot(14, new ItemStack(ItemsRegMvk.Tie));
            Inventory.SetStackInSlot(15, new ItemStack(ItemsRegMvk.ShirtBranded));
            Inventory.SetStackInSlot(16, new ItemStack(ItemsRegMvk.BackpackBranded));
            Inventory.SetStackInSlot(17, new ItemStack(ItemsRegMvk.PantsJeans));
            Inventory.SetStackInSlot(18, new ItemStack(ItemsRegMvk.BeltBranded));
            Inventory.SetStackInSlot(19, new ItemStack(ItemsRegMvk.BootsBranded));

            // Рюкзак
            //Inventory.SetStackInSlot(22, new ItemStack(Ce.Items.ItemObjects[3], 3));
            //Inventory.SetStackInSlot(23, new ItemStack(Ce.Items.ItemObjects[8]));

            // При запуске на сервере
            InvPlayer.CheckingClothes(false); // иметируем клиента, чтоб не дропнуть предметы с рюкзака
        }

        #region NBT

        protected override void _WriteToNBT(TagCompound nbt)
        {
            base._WriteToNBT(nbt);
        }

        public override void ReadFromNBT(TagCompound nbt)
        {
            base.ReadFromNBT(nbt);
            // При запуске на сервере
            InvPlayer.CheckingClothes(false); // иметируем клиента, чтоб не дропнуть предметы с рюкзака
        }

        #endregion

        /// <summary>
        /// Инициализация инвенторя
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _CreateInventory()
        {
            Inventory = InvPlayer = new InventoryPlayerMvk(this);
            InvPlayer.SlotSetted += _InventoryPlayer_SlotSetted;
            InvPlayer.SlotStorageChanged += _InventoryPlayer_SlotStorageChanged;
        }

        /// <summary>
        /// Событие слот хранилища изменён, обновляем всем кто сейчас смотрит содержимое
        /// </summary>
        private void _InventoryPlayer_SlotStorageChanged(object sender, SlotEventArgs e)
            => _server.Players.SendToAllUseBlockStorage(Inventory.GetBlockStorage(), 
                new PacketS2FSetSlot((short)e.SlotId, e.Stack));

        /// <summary>
        /// Событие изменён слот
        /// </summary>
        private void _InventoryPlayer_SlotSetted(object sender, SlotEventArgs e)
        {
            // CheckKnowledge(stack);
            SendPacket(new PacketS2FSetSlot((short)e.SlotId, e.Stack));
        }

        /// <summary>
        /// Пакет: Установки или взаимодействия с блоком
        /// </summary>
        public override void PacketPlayerBlockPlacement(PacketC08PlayerBlockPlacement packet)
        {
            // Временно устанваливаем блок
            byte currentIndex = Inventory.GetCurrentIndex();

            int idBlock = BlocksRegMvk.Debug.IndexBlock;
            if (currentIndex == 0) idBlock = BlocksRegMvk.Stone.IndexBlock;
            else if (currentIndex == 1) idBlock = BlocksRegMvk.Water.IndexBlock;
            else if (currentIndex == 2) idBlock = BlocksRegMvk.Lava.IndexBlock;
            else if (currentIndex == 3) idBlock = BlocksRegMvk.FlowerDandelion.IndexBlock;// | (1 << 28) | (4 << 24);
            else if (currentIndex == 4) idBlock = BlocksRegMvk.Glass.IndexBlock;
            else if (currentIndex == 5) idBlock = BlocksRegMvk.GlassBlue.IndexBlock;
            else if (currentIndex == 6) idBlock = BlocksRegMvk.GlassRed.IndexBlock;
            else if (currentIndex == 7) idBlock = BlocksRegMvk.SaplingBirch.IndexBlock;
            else if (currentIndex == 8) idBlock = BlocksRegMvk.SaplingOak.IndexBlock;

            // Определяем на какую сторону смотрит игрок
            Pole pole = PoleConvert.FromAngle(RotationYaw);

            WorldServer worldServer = GetWorld();
            BlockState blockState = new BlockState(idBlock);// world.GetBlockState(packet.GetBlockPos());
            BlockBase block = blockState.GetBlock();

            BlockPos blockPos = packet.GetBlockPos().Offset(packet.Side);
            // TODO::ВРЕМЕННО!!!
            blockState = block.OnBlockPlaced(worldServer, packet.GetBlockPos(), blockState, pole, packet.Facing);
            //block.OnBlockPlaced(world, packet.GetBlockPos(), blockState, packet.Side, packet.Facing);
            worldServer.SetBlockState(blockPos, blockState, worldServer.IsRemote ? 46 : 63);
            if (idBlock == BlocksRegMvk.Water.IndexBlock || idBlock == BlocksRegMvk.Lava.IndexBlock)
            {
                worldServer.SetBlockTick(blockPos, 10);
            }
            //else if (idBlock == BlocksRegMvk.SaplingBirch.IndexBlock || idBlock == BlocksRegMvk.SaplingOak.IndexBlock)
            //{
            //    worldServer.SetBlockTick(blockPos, 45); // 1.5 sec
            //}
        }

        /// <summary>
        /// Пакет: кликов по окну и контролам
        /// </summary>
        public override void PacketClickWindow(PacketC0EClickWindow packet)
        {
            // Пометка активности игрока
            MarkPlayerActive();
            EnumActionClickWindow action = (EnumActionClickWindow)packet.Action;

            if (action == EnumActionClickWindow.Close || action == EnumActionClickWindow.ThrowOutAir)
            {
                InvPlayer.ThrowOutAir();
            }

            switch (action)
            {
                case EnumActionClickWindow.OpenBoxDebug:
                    if (GetWorld().Settings is WorldSettingsIsland worldIsland)
                    {
                        InvPlayer.ServerOpenInventory(worldIsland.StorageHole);
                    }
                    //if (_server.ModServer is Games.GameModServerMvk mod)
                    //{
                    //    InvPlayer.ServerOpenInventory(mod.Hole);
                    //}
                    break;
                case EnumActionClickWindow.OpenCreativeInventory:
                    InvPlayer.ServerOpenInventory(ItemsRegMvk.Creative);
                    InvPlayer.OpenCreativeInventory(packet.Number >> 8, packet.Number & 255);
                    break;
                case EnumActionClickWindow.OpenInventory:
                    break;
                case EnumActionClickWindow.Close:
                    InvPlayer.ServerCloseInventory();
                    //_server.Players.PlayerTileClose(this);
                    break;
                case EnumActionClickWindow.ClickSlot:
                    InvPlayer.ClickInventoryOnServer(packet.Number, packet.IsRight, packet.IsShift);
                    break;
            }
        }
    }
}
