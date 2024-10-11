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
        /// Заносим данные с пакета сервера
        /// </summary>
        public void PacketChunckData(PacketS21ChunkData packet)
        {
            int chx = packet.CurrentChunkX;
            int chy = packet.CurrentChunkY;
            if (packet.IsRemoved())
            {
                // Выгружаем чанк
                _chunkMapping.Remove(chx, chy);
            }
            else
            {
                // Вносим данные в чанк
                ChunkBase chunkBase = _chunkMapping.Get(chx, chy) as ChunkBase;
                if (chunkBase == null)
                {
                    chunkBase = new ChunkBase(_worldClient, chx, chy);
                    _chunkMapping.Add(chunkBase);
                }
                // Далее тут манипулации с чанком chunkBase

            }
            _OnChunkMappingChanged();
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
