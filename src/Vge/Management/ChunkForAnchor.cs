using System.Collections.Generic;
using Vge.World;
using Vge.World.Chunk;

namespace Vge.Management
{
    /// <summary>
    /// Чанк для якоря
    /// В малювеки 1 был аналог PlayerInstance
    /// </summary>
    public class ChunkForAnchor : IChunkPosition
    {
        /// <summary>
        /// Позиция X текущего чанка
        /// </summary>
        public int CurrentChunkX { get; private set; }
        /// <summary>
        /// Позиция Y текущего чанка
        /// </summary>
        public int CurrentChunkY { get; private set; }
        /// <summary>
        /// Серверный мир к которому принадлежит чанк
        /// </summary>
        public readonly WorldServer World;

        /// <summary>
        /// Список якорей которые имеют этот чанк в обзоре
        /// </summary>
        private readonly List<IAnchor> _anchors = new List<IAnchor>();
        /// <summary>
        /// Список игроков которые имеют этот чанк в обзоре
        /// </summary>
        private readonly List<PlayerServer> _players = new List<PlayerServer>();
        /// <summary>
        /// Предыдущее игровой такт, для определения сколько времени провели в этом чанке якоря
        /// </summary>
        private uint _previousGameTakt;

        public ChunkForAnchor(WorldServer world, int chunkPosX, int chunkPosY)
        {
            World = world;
            CurrentChunkX = chunkPosX;
            CurrentChunkY = chunkPosY;
        }

        /// <summary>
        /// Проверить имеется ли якорь в этом чанке
        /// </summary>
        public bool Contains(IAnchor anchor) => _anchors.Contains(anchor);
        /// <summary>
        /// Проверить имеется ли такой игрок в этом чанке
        /// </summary>
        public bool ContainsPlayer(PlayerServer player) => _players.Contains(player);

        /// <summary>
        /// Количество якорей в этом чанке
        /// </summary>
        public int CountAnchor() => _anchors.Count;
        /// <summary>
        /// Количество игроков в этом чанке
        /// </summary>
        public int CountPlayer() => _players.Count;

        /// <summary>
        /// Добавить якорь в конкретный чанк
        /// </summary>
        public void AddAnchor(IAnchor anchor)
        {
            if (!_anchors.Contains(anchor))
            {
                if (_anchors.Count == 0) _previousGameTakt = World.Server.TickCounter;
                _anchors.Add(anchor);
                anchor.AddChunk(CurrentChunkX, CurrentChunkY);
            }
            // Добавить игрока если якорь является игроком
            if (anchor is PlayerServer player)
            {
                if (!_players.Contains(player))
                {
                    _players.Add(player);
                }
            }
        }

        /// <summary>
        /// Убрать якорь с конкретного чанка, вернёт true если больше нет якорей в чанке
        /// </summary>
        public bool RemoveAnchor(IAnchor anchor)
        {
            // Удалить игрока если якорь является игроком
            if (anchor is PlayerServer player)
            {
                if (_players.Contains(player))
                {
                    _players.Remove(player);
                }
            }

            if (_anchors.Contains(anchor))
            {
                _anchors.Remove(anchor);
                if (_anchors.Count == 0)
                {
                    // Если удалили последний якорь
                    _IncreaseInhabitedTime();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Увеличить время проживания у чанка
        /// </summary>
        private void _IncreaseInhabitedTime()
        {
            ChunkBase chunk = World.GetChunk(CurrentChunkX, CurrentChunkY);
            if (chunk != null)
            {
                uint countInhabitedTakt = World.Server.TickCounter - _previousGameTakt;
                _previousGameTakt = World.Server.TickCounter;
                chunk.SetInhabitedTime(chunk.InhabitedTakt + countInhabitedTakt);
            }
        }

        public override string ToString() => CurrentChunkX + ":" + CurrentChunkY + " " + _anchors.Count;
    }
}
