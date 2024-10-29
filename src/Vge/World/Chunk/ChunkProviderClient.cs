using System;
using Vge.Network.Packets.Server;
using Vge.Renderer.World;

namespace Vge.World.Chunk
{
    /// <summary>
    /// Объект Клиент который хранит и отвечает за кэш чанков
    /// </summary>
    public class ChunkProviderClient : ChunkProvider
    {
        /// <summary>
        /// Клиентский мир
        /// </summary>
        private readonly WorldClient _worldClient;

        public ChunkProviderClient(WorldClient world) : base(world)
            => _worldClient = world;

        /// <summary>
        /// Выгрузить чанк
        /// </summary>
        public void UnloadChunk(ChunkRender chunk)
        {
            if (chunk != null)
            {
                if (chunk.IsChunkPresent)
                {
                    chunk.OnChunkUnload();
                }
                chunk.Dispose();
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
                UnloadChunk(_chunkMapping.Get(chx, chy) as ChunkRender);
            }
            else
            {
                // Вносим данные в чанк
                ChunkRender chunk = _chunkMapping.Get(chx, chy) as ChunkRender;
                if (chunk == null)
                {
                    chunk = new ChunkRender(_worldClient, chx, chy);
                    _chunkMapping.Add(chunk);
                }

                chunk.SetBinary();// packet.GetBuffer(), packet.IsBiom(), packet.GetFlagsYAreas());

                // Далее тут манипулации с чанком chunkBase
                //System.Threading.Thread.Sleep(20);

            }
            if (Ce.IsDebugDrawChunks)
            {
                _OnChunkMappingChanged();
            }
        }

        /// <summary>
        /// Получить клиентский чанк по координатам чанка
        /// </summary>
        public ChunkRender GetChunkRender(int x, int y)
             => _chunkMapping.Get(x, y) as ChunkRender;

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
