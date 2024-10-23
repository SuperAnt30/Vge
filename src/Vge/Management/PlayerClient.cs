using Vge.Games;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;
using WinGL.Util;

namespace Vge.Management
{
    /// <summary>
    /// Объект игрока, на клиенте
    /// </summary>
    public class PlayerClient : PlayerBase
    {
        /// <summary>
        /// Матрица просмотра Projection * LookAt
        /// </summary>
        public float[] View { get; private set; }

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
            UpView();
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
                _game.WorldRender.SetOverviewChunk(overviewChunk);
                // Отправим обзор 
                _game.TrancivePacket(new PacketC15PlayerSetting(OverviewChunk));
                if (isSaveOptions)
                {
                    Options.OverviewChunk = OverviewChunk;
                    new OptionsFile().Save();
                }
            }
        }

        /// <summary>
        /// Обновить матрицу камеры
        /// </summary>
        public void UpView()
        {
            Vector3 front = new Vector3(.1f, -.98f, 0); //GetLookFrame(timeIndex).normalize();
            Vector3 up = new Vector3(0, 1, 0);
            Vector3 pos = new Vector3(0, 96, 0);
            Mat4 look = Glm.LookAt(pos, pos + front, up);
            Mat4 projection = Glm.Perspective(65f, (float)Gi.Width / (float)Gi.Height, 
                0.01f, 16 * 22f);
            View = (projection * look).ToArray();
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
