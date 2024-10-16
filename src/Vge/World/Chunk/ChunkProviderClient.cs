using System;
using Vge.Network.Packets.Server;

namespace Vge.World.Chunk
{
    /// <summary>
    /// Объект Клиент который хранит и отвечает за кэш чанков
    /// </summary>
    public class ChunkProviderClient : ChunkProvider
    {
        /// <summary>
        /// Посредник серверного чанка
        /// </summary>
        private readonly WorldClient _worldClient;

        public ChunkProviderClient(WorldClient world) : base(world)
        {
            _worldClient = world;
        }

        /// <summary>
        /// Выгрузить чанк
        /// </summary>
        public void UnloadChunk(ChunkBase chunk)
        {
            if (chunk != null)
            {
                if (chunk.IsChunkPresent)
                {
                    chunk.OnChunkUnload();
                }
                _chunkMapping.Remove(chunk.CurrentChunkX, chunk.CurrentChunkY);
            }
        }

        #region Packet

        /// <summary>
        /// Заносим данные с пакета сервера
        /// </summary>
        public void PacketChunckData(PacketS21ChunkData packet)
        {
            int chx = packet.CurrentChunkX;
            int chy = packet.CurrentChunkY;
            if (packet.IsRemoved())
            {
                // Выгружаем чанк
                UnloadChunk(_chunkMapping.Get(chx, chy) as ChunkBase);
            }
            else
            {
                // Вносим данные в чанк
                ChunkBase chunk = _chunkMapping.Get(chx, chy) as ChunkBase;
                if (chunk == null)
                {
                    chunk = new ChunkBase(_worldClient, chx, chy);
                    _chunkMapping.Add(chunk);
                }
                // Далее тут манипулации с чанком chunkBase
                //System.Threading.Thread.Sleep(20);

            }
            if (Ce.IsDebugDrawChunks)
            {
                _OnChunkMappingChanged();
            }
        }

        #endregion

        public override string ToString() => string.Format("Ch:{0}|{1} Dr:{2}",
               _chunkMapping.Count, _chunkMapping.RegionCount, -1);

        #region Event

        /// <summary>
        /// Событие изменены чанки
        /// </summary>
        public event EventHandler ChunkMappingChanged;
        private void _OnChunkMappingChanged()
            => ChunkMappingChanged?.Invoke(this, new EventArgs());

        #endregion
    }
}
