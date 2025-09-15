using System;
using Vge.Entity;
using Vge.Entity.Inventory;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Network.Packets;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;

namespace Vge.Network
{
    /// <summary>
    /// Обработка клиентсиких пакетов для сервером
    /// </summary>
    public class ProcessClientPackets
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public readonly GameBase Game;
        /// <summary>
        /// По сети, false значит сервер этого же клиента
        /// </summary>
        public readonly bool IsNet;

        /// <summary>
        /// Трафик в байтах
        /// </summary>
        public long Traffic { get; private set; } = 0;

        /// <summary>
        /// Массив очередей пакетов
        /// </summary>
        private readonly DoubleList<IPacket> _packets = new DoubleList<IPacket>();

        /// <summary>
        /// Объект который из буфера данных склеивает пакеты
        /// </summary>
        private ReadPacket _readPacket = new ReadPacket();

        /// <summary>
        /// Время в мс сколько загружались чанки у клиента
        /// </summary>
        private long _timeReceiveChunk;
        /// <summary>
        /// Какое количество загружало чанков у клиента
        /// </summary>
        private byte _countReceiveChunk;

        public ProcessClientPackets(GameBase client)
        {
            Game = client;
            IsNet = client is GameNet;
        }

        /// <summary>
        /// Передача данных для клиента
        /// </summary>
        public void ReceiveBuffer(byte[] buffer, int count)
        {
            Traffic += count + Ce.SizeHeaderTCP;
            _readPacket.SetBuffer(buffer);
            _ReceivePacket(_readPacket.Receive(PacketsInit.InitServer(buffer[0])));
        }

        private void _ReceivePacket(IPacket packet)
        {
            byte index = packet.Id;
            if (index <= 2 || index == 0x20)
            {
                //if (IsNet) Game.Log.Client("Packet {0:X}", index);
                switch (index)
                {
                    case 0x00: _Handle00Pong((Packet00PingPong)packet); break;
                    case 0x01: _Handle01KeepAlive((Packet01KeepAlive)packet); break;
                    case 0x02: _Handle02LoadingGame((PacketS02LoadingGame)packet); break;
                    case 0x20: _Handle20ChunkSend((PacketS20ChunkSend)packet); break;
                }
            }
            else
            {
                // Мир есть, заносим в пакет с двойным буфером, для обработки в такте
                _packets.Add(packet);
            }
        }

        /// <summary>
        /// Передача данных для клиента в последовотельности игрового такта
        /// </summary>
        private void _UpdateReceivePacket(IPacket packet)
        {
            //if (IsNet) Game.Log.Client("Packet {0:X}", packet.Id);
            switch (packet.Id)
            {
                case 0x03: _Handle03JoinGame((PacketS03JoinGame)packet); break;
                case 0x04: _Handle04TimeUpdate((PacketS04TickUpdate)packet); break;
                case 0x05: _Handle05Ready((PacketS05Ready)packet); break;
                case 0x06: _Handle06PlayerEntryRemove((PacketS06PlayerEntryRemove)packet); break;
                case 0x07: _Handle07RespawnInWorld((PacketS07RespawnInWorld)packet); break;
                case 0x08: _Handle08PlayerPosLook((PacketS08PlayerPosLook)packet); break;
                case 0x0B: _Handle0BAnimation((PacketS0BAnimation)packet); break;
                case 0x0C: _Handle0CSpawnPlayer((PacketS0CSpawnPlayer)packet); break;
                case 0x0F: _Handle0FSpawnMob((PacketS0FSpawnMob)packet); break;
                case 0x13: _Handle13DestroyEntities((PacketS13DestroyEntities)packet); break;
                case 0x14: _Handle14EntityMotion((PacketS14EntityMotion)packet); break;
                case 0x1C: _Handle1CEntityMetadata((PacketS1CEntityMetadata)packet); break;
                case 0x21: _Handle21ChunkData((PacketS21ChunkData)packet); break;
                case 0x22: _Handle22MultiBlockChange((PacketS22MultiBlockChange)packet); break;
                case 0x23: _Handle23BlockChange((PacketS23BlockChange)packet); break;
                case 0x2F: _Handle2FSetSlot((PacketS2FSetSlot)packet); break;
                case 0x30: _Handle30WindowItems((PacketS30WindowItems)packet); break;
                case 0x3A: _Handle3AMessage((PacketS3AMessage)packet); break;
            }
        }

        /// <summary>
        /// Игровой такт клиента
        /// </summary>
        public void Update()
        {
            if (!_packets.Empty())
            {
                _packets.Step();
                int count = _packets.CountBackward;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        _UpdateReceivePacket(_packets.GetNext());
                    }
                    if (_countReceiveChunk > 0)
                    {
                        Game.TrancivePacket(new PacketC20AcknowledgeChunks((int)_timeReceiveChunk, _countReceiveChunk, false));
                        _countReceiveChunk = 0;
                        _timeReceiveChunk = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Отправить пакет сообщения основному игроку через защищённую от потоков
        /// </summary>
        public void PlayerOwnerPacketMessage(string message)
            => _packets.Add(new PacketS3AMessage(message));

        /// <summary>
        /// Очистить пакеты в двойной буферизации
        /// </summary>
        public void Clear() => _packets.Clear();

        #region Handles

        /// <summary>
        /// Пакет связи
        /// </summary>
        private void _Handle00Pong(Packet00PingPong packet) 
            => Game.Player.SetPing(packet.ClientTime);

        /// <summary>
        /// KeepAlive
        /// </summary>
        private void _Handle01KeepAlive(Packet01KeepAlive packet)
            => Game.TrancivePacket(packet);

        /// <summary>
        /// Загрузка игры
        /// </summary>
        private void _Handle02LoadingGame(PacketS02LoadingGame packet)
            => Game.PacketLoadingGame(packet);

        /// <summary>
        /// Пакет соединения игрока с сервером
        /// </summary>
        private void _Handle03JoinGame(PacketS03JoinGame packet)
            => Game.PlayerOnTheServer(packet.Index, packet.Uuid);

        /// <summary>
        /// Пакет синхронизации времени с сервером
        /// </summary>
        private void _Handle04TimeUpdate(PacketS04TickUpdate packet)
            => Game.World.Settings.Calendar.SetTickCounter(packet.Tick);

        /// <summary>
        /// Пакет готовность игры
        /// </summary>
        private void _Handle05Ready(PacketS05Ready packet)
            => Game.GameStartingNet();

        /// <summary>
        /// Пакет игрок зашёл или вышел из сервера, для чата
        /// </summary>
        private void _Handle06PlayerEntryRemove(PacketS06PlayerEntryRemove packet)
            => Game.PlayerEntryRemove(packet);

        /// <summary>
        /// Пакет Возраждение в мире
        /// </summary>
        private void _Handle07RespawnInWorld(PacketS07RespawnInWorld packet)
            => Game.Player.PacketRespawnInWorld(packet);

        /// <summary>
        /// Пакет расположения игрока, при старте, телепорт, рестарте и тп
        /// </summary>
        private void _Handle08PlayerPosLook(PacketS08PlayerPosLook packet)
        {
            if (packet.Impulse)
            {
                Game.Player.Physics.ImpulseX = packet.X;
                Game.Player.Physics.ImpulseY = packet.Y;
                Game.Player.Physics.ImpulseZ = packet.Z;
            }
            else
            {
                Game.Player.PosX = packet.X;
                Game.Player.PosY = packet.Y;
                Game.Player.PosZ = packet.Z;
                Game.Player.RotationPrevYaw = Game.Player.RotationYaw = packet.Yaw;
                Game.Player.RotationPrevPitch = Game.Player.RotationPitch = packet.Pitch;
            }
            Game.Player.Physics.AwakenPhysics();
            Debug.Player = Game.Player.GetChunkPosition();
        }

        /// <summary>
        /// Пакет анимации сущности
        /// </summary>
        private void _Handle0BAnimation(PacketS0BAnimation packet)
        {
            EntityBase entity = Game.World.GetEntityByID(packet.EntityId);
            if (entity != null && entity.Render != null)
            {
                entity.Render.SetMovingFlags(packet.Animation);
            }
        }

        /// <summary>
        /// Пакет спавна других игроков
        /// </summary>
        private void _Handle0CSpawnPlayer(PacketS0CSpawnPlayer packet)
        {
            // Игрок в видимости обзора, надо заспавнить
            // Или после смерти
            if (Game.Player.IdWorld == packet.IdWorld)
            {
                // После проверки, что оба в одном мире
                PlayerClient player = Game.ModClient.CreatePlayerClient(packet);

                player.SpawnPosition(packet.X, packet.Y, packet.Z);
                player.OnGround = packet.OnGround;
                player.SpawnRotation(packet.Yaw, packet.Pitch);
                player.Inventory.SetOutside(packet.Stacks);
                player.MetaData.UpdateWatchedObjectsFromList(packet.Data);

                Game.World.SpawnEntityInWorld(player);
            }
        }

        /// <summary>
        ///  Пакет спавна мобов
        /// </summary>
        private void _Handle0FSpawnMob(PacketS0FSpawnMob packet)
        {
            EntityBase entity = Ce.Entities.CreateEntityClient(packet.IndexEntity, Game.WorldRender.Entities);

            entity.SetEntityId(packet.Index);
            entity.SpawnPosition(packet.X, packet.Y, packet.Z);
            entity.OnGround = packet.OnGround;
            if (entity is EntityLiving entityLiving)
            {
                entityLiving.SpawnRotation(packet.Yaw, packet.Pitch);
                if (entityLiving.Inventory != null)
                {
                    entityLiving.Inventory.SetOutside(packet.Stacks);
                }
            }
            entity.MetaData.UpdateWatchedObjectsFromList(packet.Data);
            Game.World.SpawnEntityInWorld(entity);
        }

        /// <summary>
        /// Пакет удаление сущностей
        /// </summary>
        private void _Handle13DestroyEntities(PacketS13DestroyEntities packet)
        {
            int count = packet.Ids.Length;
            for (int i = 0; i < count; i++)
            {
                 Game.World.RemoveEntityInWorld(packet.Ids[i]);
            }
        }

        /// <summary>
        /// Пакет перемещения сущности
        /// </summary>
        private void _Handle14EntityMotion(PacketS14EntityMotion packet)
        {
           // Game.World
            EntityBase entity = Game.World.GetEntityByID(packet.EntityId);
            if (entity != null)
            {
                entity.PosServerX = packet.X;
                entity.PosServerY = packet.Y;
                entity.PosServerZ = packet.Z;
                
                entity.OnGround = packet.OnGround;
                entity.SetPhysicsSleepDebug(packet.Sleep);

                if (entity is EntityLiving entityLiving)
                {
                    entityLiving.RotationServerYaw = packet.Yaw;
                    entityLiving.RotationServerPitch = packet.Pitch;

                    //entityLiving.SetMotionServer(
                    //    packet.GetPos(), packet.GetYaw(), packet.GetPitch(),
                    //    packet.OnGround());
                }
                //else if (entity is EntityItem entityItem)
                //{
                //    entityItem.SetMotionServer(packet.GetPos(), packet.OnGround());
                //}
                //else if (entity is EntityThrowable entityThrowable)
                //{
                //    entityThrowable.SetMotionServer(packet.GetPos(), packet.OnGround());
                //}
            }
        }

        /// <summary>
        /// Пакет дополнительных данных сущности
        /// </summary>
        private void _Handle1CEntityMetadata(PacketS1CEntityMetadata packet)
        {
            EntityBase entity = Game.World.GetEntityByID(packet.EntityId);
            if (entity != null && packet.Data.Length > 0)
            {
                entity.MetaData.UpdateWatchedObjectsFromList(packet.Data);
                // TODO::2025-06-23 Вынести в анимацию!!!
                if (entity is EntityLiving entityLiving)
                {
                    if (entityLiving.IsSneaking()) entityLiving.Sitting(); else entityLiving.Standing();
                }
            }
        }

        /// <summary>
        /// Замер скорости закачки чанков.
        /// Обрабатываем сразу, не дожидаясь такта
        /// </summary>
        private void _Handle20ChunkSend(PacketS20ChunkSend packet)
            => Game.Player.PacketChunckSend(packet);

        /// <summary>
        /// Пакет изменённые псевдо чанки
        /// </summary>
        private void _Handle21ChunkData(PacketS21ChunkData packet)
        {
            long time = Game.Time();
            if (Game.World.ChunkPrClient.PacketChunckData(packet))
            {
                // Считаем время
                _countReceiveChunk++;
                _timeReceiveChunk += Game.Time() - time;
            }
            Debug.BlockChange = "ChunkData: " + packet.FlagsYAreas;
        }

        /// <summary>
        /// Пакет много изменённых блоков
        /// </summary>
        private void _Handle22MultiBlockChange(PacketS22MultiBlockChange packet)
        {
            packet.ReceivedBlocks(Game.World);
            Debug.BlockChange = "MultiBlock: " + packet.Count();
        }

        /// <summary>
        /// Пакет один изменённый блок
        /// </summary>
        private void _Handle23BlockChange(PacketS23BlockChange packet)
        {
            Game.World.SetBlockState(packet.GetBlockPos(), packet.GetBlockState(), 4);
            Debug.BlockChange = "BlockChange";
        }

        /// <summary>
        /// Пакет управления передвежением и изменением слота
        /// </summary>
        private void _Handle2FSetSlot(PacketS2FSetSlot packet)
        {
            Slot slot = packet.GetSlot();
            //if (slot < 100 || slot == 255)
            //{
            //    ClientMain.Player.InventPlayer.SetInventorySlotContents(slot, packet.GetItemStack());
            //    if (slot == ClientMain.Player.InventPlayer.CurrentItem)
            //    {
            //        ClientMain.Player.ItemInWorldManagerDestroyAbout();
            //    }
            //}
            //else
            //{
            //    // Пришёл стак для склада
            //    ClientMain.Screen.AcceptNetworkPackage(packet);
            //}
        }

        /// <summary>
        /// Пакет окна списка предметов, стартово все придметы игрока
        /// </summary>
        private void _Handle30WindowItems(PacketS30WindowItems packet)
        {
            if (packet.IsInventory)
            {
                Game.Player.Inventory.SetAll(packet.Stacks);
            }
            else
            {
              //  ClientMain.Screen.AcceptNetworkPackage(packet);
            }
        }

        /// <summary>
        /// Пакет получения сообщения с сервера
        /// </summary>
        private void _Handle3AMessage(PacketS3AMessage packet)
            => Game.Player.PacketMessage(packet);

#endregion
    }
}
