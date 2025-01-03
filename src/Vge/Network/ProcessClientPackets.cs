﻿using Vge.Entity;
using Vge.Games;
using Vge.Network.Packets;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;
using Vge.World.Block;

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
        public GameBase Game { get; private set; }

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

        public ProcessClientPackets(GameBase client) => Game = client;

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
            switch (packet.Id)
            {
                case 0x03: _Handle03JoinGame((PacketS03JoinGame)packet); break;
                case 0x04: _Handle04TimeUpdate((PacketS04TimeUpdate)packet); break;
                case 0x05: _Handle05TableBlocks((PacketS05TableBlocks)packet); break;
                case 0x07: _Handle07RespawnInWorld((PacketS07RespawnInWorld)packet); break;
                case 0x08: _Handle08PlayerPosLook((PacketS08PlayerPosLook)packet); break;
                case 0x0C: _Handle0CSpawnPlayer((PacketS0CSpawnPlayer)packet); break;
                case 0x13: _Handle13DestroyEntities((PacketS13DestroyEntities)packet); break;
                case 0x14: _Handle14EntityMotion((PacketS14EntityMotion)packet); break;
                case 0x21: _Handle21ChunkData((PacketS21ChunkData)packet); break;
                case 0x22: _Handle22MultiBlockChange((PacketS22MultiBlockChange)packet); break;
                case 0x23: _Handle23BlockChange((PacketS23BlockChange)packet); break;
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
        private void _Handle04TimeUpdate(PacketS04TimeUpdate packet)
            => Game.SetTickCounter(packet.Time);

        /// <summary>
        /// Пакет передать таблицу блоков
        /// </summary>
        private void _Handle05TableBlocks(PacketS05TableBlocks packet)
            => BlocksReg.Correct(new CorrectTable(packet.Blocks));

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
            Game.Player.PosX = packet.X;
            Game.Player.PosY = packet.Y;
            Game.Player.PosZ = packet.Z;
            Game.Player.RotationYaw = packet.Yaw;
            Game.Player.RotationPitch = packet.Pitch;

            Debug.Player = Game.Player.GetChunkPosition();
        }

        /// <summary>
        /// Пакет спавна других игроков
        /// </summary>
        private void _Handle0CSpawnPlayer(PacketS0CSpawnPlayer packet)
        {
            // Удачный вход сетевого игрока, типа приветствие
            // Или после смерти
            //EntityPlayerMP entity = new EntityPlayerMP(ClientMain.World);
            //entity.SetDataPlayer(packet.GetId(), packet.GetUuid(), packet.GetName());
            //entity.SetPosLook(packet.GetPos(), packet.GetYaw(), packet.GetPitch());
            //entity.Inventory.SetCurrentItemAndCloth(packet.GetStacks());
            //ArrayList list = packet.GetList();
            //if (list != null && list.Count > 0)
            //{
            //    entity.MetaData.UpdateWatchedObjectsFromList(list);
            //}
            //ClientMain.World.AddEntityToWorld(entity.Id, entity);
        }

        /// <summary>
        /// Пакет удаление сущностей
        /// </summary>
        private void _Handle13DestroyEntities(PacketS13DestroyEntities packet)
        {
            int count = packet.Ids.Length;
            for (int i = 0; i < count; i++)
            {
                //ClientMain.World.RemoveEntityFromWorld(packet.Ids[i]);
            }
        }

        /// <summary>
        /// Пакет перемещения сущности
        /// </summary>
        private void _Handle14EntityMotion(PacketS14EntityMotion packet)
        {
           // Game.World
            //EntityBase entity = ClientMain.World.GetEntityByID(packet.Index);
            //if (entity != null)
            {
                //if (entity is EntityLiving entityLiving)
                //{
                //    entityLiving.SetMotionServer(
                //        packet.GetPos(), packet.GetYaw(), packet.GetPitch(),
                //        packet.OnGround());
                //}
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
        /// Пакет получения сообщения с сервера
        /// </summary>
        private void _Handle3AMessage(PacketS3AMessage packet)
            => Game.Player.PacketMessage(packet);

        #endregion
    }
}
