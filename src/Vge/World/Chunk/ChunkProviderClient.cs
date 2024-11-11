using System;
using System.Collections.Generic;
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
                _chunkMapping.Remove(chunk.CurrentChunkX, chunk.CurrentChunkY);
                chunk.DisposeMesh();
            }
        }

        #region Packet

        /// <summary>
        /// Заносим данные с пакета сервера, возвращает true если создать чанк
        /// </summary>
        public bool PacketChunckData(PacketS21ChunkData packet)
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
                ChunkRender chunkRender = _chunkMapping.Get(chx, chy) as ChunkRender;
                bool isNew = chunkRender == null;
                if (isNew)
                {
                    chunkRender = new ChunkRender(_worldClient, chx, chy);
                    _chunkMapping.Add(chunkRender);
                    _worldClient.AreaModifiedToRender(chx - 1, 0, chy - 1, chx + 1, Settings.NumberSections, chy + 1);
                }

                chunkRender.SetBinaryZip(packet.BufferRead, packet.IsBiom, packet.FlagsYAreas);
                //chunkRender.SetBinary(packet.BufferRead, packet.IsBiom, packet.FlagsYAreas);
                
                if (isNew || packet.IsBiom)
                {
                    //chunk.Light.GenerateHeightMap();
                    if (Ce.IsDebugDrawChunks) _OnChunkMappingChanged();
                    return true;
                }
            }
            if (Ce.IsDebugDrawChunks) _OnChunkMappingChanged();
            return false;
        }

        /// <summary>
        /// Получить клиентский чанк по координатам чанка
        /// </summary>
        public ChunkRender GetChunkRender(int x, int y)
             => _chunkMapping.Get(x, y) as ChunkRender;

        #endregion

        /// <summary>
        /// Останавливаем
        /// </summary>
        public void Stoping()
        {
            // Получить список всех чанков
            List<IChunkPosition> positions = _chunkMapping.GetList();
            // Выгружаем чанки
            foreach (ChunkRender chunkRender in positions)
            {
                UnloadChunk(chunkRender);
            }
        }

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
