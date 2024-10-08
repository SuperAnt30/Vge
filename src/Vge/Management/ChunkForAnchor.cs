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
        /// Список мировых якорей которые имеют этот чанк в обзоре
        /// </summary>
        private readonly List<WorldAnchor> _worldAnchors = new List<WorldAnchor>();
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
        /// Проверить имеется ли мировой якорь в этом чанке
        /// </summary>
        public bool ContainsWorld(WorldAnchor worldAnchor) => _worldAnchors.Contains(worldAnchor);
        /// <summary>
        /// Проверить имеется ли такой игрок в этом чанке
        /// </summary>
        public bool ContainsPlayer(PlayerServer player) => _players.Contains(player);

        /// <summary>
        /// Количество якорей в этом чанке
        /// </summary>
        public int CountAnchor() => _anchors.Count;
        /// <summary>
        /// Количество мировых якорей в этом чанке
        /// </summary>
        public int CountWorld() => _worldAnchors.Count;
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
            // Добавить игрока если является
            if (anchor is PlayerServer player)
            {
                if (!_players.Contains(player))
                {
                    _players.Add(player);
                }
            }
            // Добавить мировой якорь если является
            else if (anchor is WorldAnchor worldAnchor)
            {
                if (!_worldAnchors.Contains(worldAnchor))
                {
                    _worldAnchors.Add(worldAnchor);
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
                _players.Remove(player);
            }
            // Удалить мировой якорь если является
            else if (anchor is WorldAnchor worldAnchor)
            {
                _worldAnchors.Remove(worldAnchor);
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
                anchor.RemoveChunk(CurrentChunkX, CurrentChunkY);
            }
            return false;
        }

        /// <summary>
        /// Продлить жизнь если один мировой якорь 
        /// </summary>
        public void ProlongLife()
        {
            if (_anchors.Count == 1 && _worldAnchors.Count == 1)
            {
                _worldAnchors[0].ProlongLife();
            }
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
