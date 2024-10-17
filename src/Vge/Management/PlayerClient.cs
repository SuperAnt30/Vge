using Vge.Games;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;

namespace Vge.Management
{
    /// <summary>
    /// Объект игрока, на клиенте
    /// </summary>
    public class PlayerClient : PlayerBase
    {
        /// <summary>
        /// Класс  игры
        /// </summary>
        private readonly GameBase _game;
        /// <summary>
        /// Время запуска партии кусков
        /// </summary>
        private long _timeStartBatchChunks;
        /// <summary>
        /// Время мс на загрузку чанков
        /// </summary>
        private int _batchChunksTime;
        /// <summary>
        /// Количество на загрузку чанков
        /// </summary>
        private byte _batchChunksQuantity;

        public PlayerClient(GameBase game)
        {
            _game = game;
            Login = game.ToLoginPlayer();
            Token = game.ToTokenPlayer();
        }

        /// <summary>
        /// Получить время в милисекундах
        /// </summary>
        protected override long _Time() => _game.Time();

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public void SetOverviewChunk(byte overviewChunk, bool isSaveOptions)
        {
            if (OverviewChunk != overviewChunk)
            {
                SetOverviewChunk(overviewChunk);
                // Отправим обзор 
                _game.TrancivePacket(new PacketC15PlayerSetting(OverviewChunk));
                if (isSaveOptions)
                {
                    Options.OverviewChunk = OverviewChunk;
                    new OptionsFile().Save();
                }
            }
        }

        #region Packet

        /// <summary>
        /// Пакет Возраждение в мире
        /// </summary>
        public void PacketRespawnInWorld(PacketS07RespawnInWorld packet)
        {
            IdWorld = packet.IdWorld;
            _game.World.ChunkPr.SetHeightChunks(packet.NumberChunkSections);
        }

        /// <summary>
        /// Замер скорости закачки чанков
        /// </summary>
        public void PacketChunckSend(PacketS20ChunkSend packet)
        {
            if (packet.Start)
            {
                // Начало замера
                _timeStartBatchChunks = _Time();
            }
            else
            {
                // Закончили замер
                _batchChunksTime = (int)(_Time() - _timeStartBatchChunks);
                _batchChunksQuantity = packet.Quantity;
                _game.TrancivePacket(new PacketC20AcknowledgeChunks(_batchChunksTime, _batchChunksQuantity));
            }
        }

        #endregion

        public override string ToString()
        {
            return Login + " " + chPos + " O:" + OverviewChunk 
                + " batch:" + _batchChunksQuantity + "|" + _batchChunksTime + "mc";
        }
    }
}
