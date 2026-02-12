using System.Collections.Generic;
using Vge.Entity.Player;
using Vge.Network;
using Vge.Network.Packets.Server;
using Vge.World;
using Vge.World.Block;
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
        /// Количество блоков отправляемые за так изменённые в чанке
        /// </summary>
        private const int _CountMultyBlocks = 1024; // было 4096

        /// <summary>
        /// Серверный мир к которому принадлежит чанк
        /// </summary>
        public readonly WorldServer World;

        /// <summary>
        /// Дополнительные данные
        /// </summary>
        public object Tag { get; }

        /// <summary>
        /// Позиция X текущего чанка
        /// </summary>
        public int CurrentChunkX { get; private set; }
        /// <summary>
        /// Позиция Y текущего чанка
        /// </summary>
        public int CurrentChunkY { get; private set; }
        /// <summary>
        /// Сколько блоков надо обновить до _CountMultyBlocks шт
        /// </summary>
        public int NumBlocksToUpdate { get; private set; }

        /// <summary>
        /// Список якорей (не игроки) которые имеют этот чанк в обзоре
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
        /// <summary>
        /// Массив каких блоков надо обновить
        /// </summary>
        private readonly int[] _locationOfBlockChange = new int[_CountMultyBlocks];
        /// <summary>
        /// Флаг какие псевдо чанки надо обновлять
        /// </summary>
        private uint _flagsYAreasToUpdate;


        public ChunkForAnchor(WorldServer world, int chunkPosX, int chunkPosY)
        {
            World = world;
            CurrentChunkX = chunkPosX;
            CurrentChunkY = chunkPosY;
        }

        /// <summary>
        /// Проверить имеется ли якорь (не игрок) в этом чанке
        /// </summary>
        public bool Contains(IAnchor anchor) => _anchors.Contains(anchor);
        /// <summary>
        /// Проверить имеется ли такой игрок в этом чанке
        /// </summary>
        public bool ContainsPlayer(PlayerServer player) => _players.Contains(player);

        /// <summary>
        /// Количество якорей (не игроков) в этом чанке
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
            // Добавить игрока если является
            if (anchor is PlayerServer player)
            {
                if (!_players.Contains(player))
                {
                    if (_anchors.Count == 0 && _players.Count == 0)
                    {
                        _previousGameTakt = World.TickCounter;
                    }
                    _players.Add(player);
                    player.AddChunk(CurrentChunkX, CurrentChunkY);
                }
            }
            else if (!_anchors.Contains(anchor))
            {
                if (_anchors.Count == 0 && _players.Count == 0)
                {
                    _previousGameTakt = World.TickCounter;
                }
                _anchors.Add(anchor);
                anchor.AddChunk(CurrentChunkX, CurrentChunkY);
            }
        }

        /// <summary>
        /// Убрать якорь с конкретного чанка, вернёт true если больше нет якорей в чанке
        /// </summary>
        public bool RemoveAnchor(IAnchor anchor)
        {
            // Удалить игрока если якорь является игроком
            bool b = false;
            if (anchor is PlayerServer player)
            {
                if (_players.Contains(player))
                {
                    _players.Remove(player);
                    player.RemoveChunk(CurrentChunkX, CurrentChunkY);
                    b = true;
                }
            }
            else if (_anchors.Contains(anchor))
            {
                _anchors.Remove(anchor);
                anchor.RemoveChunk(CurrentChunkX, CurrentChunkY);
                b = true;
            }
            if (b && _anchors.Count == 0 && _players.Count == 0)
            {
                // Если удалили последний якорь
                _IncreaseInhabitedTime();

                return true;
            }
            return false;
        }

        /// <summary>
        /// Увеличить время проживания у чанка
        /// </summary>
        private void _IncreaseInhabitedTime()
        {
            ChunkServer chunk = World.GetChunkServer(CurrentChunkX, CurrentChunkY);
            if (chunk != null)
            {
                uint countInhabitedTick = World.TickCounter - _previousGameTakt;
                _previousGameTakt = World.TickCounter;
                chunk.SetInhabitedTick(chunk.InhabitedTick + countInhabitedTick);
            }
        }

        /// <summary>
        /// Увеличить время проживания у чанка
        /// </summary>
        public void ProcessChunk() => _IncreaseInhabitedTime();

        /// <summary>
        /// Обновление в игровом такте
        /// </summary>
        public void Update()
        {
            if (NumBlocksToUpdate != 0)
            {
                IPacket packet;
                if (NumBlocksToUpdate >= _CountMultyBlocks)
                {
                    ChunkBase chunk = World.GetChunk(CurrentChunkX, CurrentChunkY);
                    if (chunk != null && chunk.IsSendChunk)
                    {
                        packet = new PacketS21ChunkData(chunk, false, _flagsYAreasToUpdate);
                        for (int i = 0; i < _players.Count; i++)
                        {
                            if (!_players[i].IsLoadingChunks(CurrentChunkX, CurrentChunkY))
                            {
                                _players[i].SendPacket(packet);
                            }
                        }
                    }
                }
                else
                {
                    if (NumBlocksToUpdate == 1)
                    {
                        int index = _locationOfBlockChange[0];
                        packet = new PacketS23BlockChange(World, new BlockPos(
                            CurrentChunkX << 4 | ((index >> 4) & 15),
                            index >> 8,
                            CurrentChunkY << 4 | (index & 15)
                        ));
                    }
                    else
                    {
                        packet = new PacketS22MultiBlockChange(NumBlocksToUpdate,
                                _locationOfBlockChange, World.GetChunk(CurrentChunkX, CurrentChunkY));
                    }

                    for (int i = 0; i < _players.Count; i++)
                    {
                        _players[i].SendPacket(packet);
                    }
                }
                NumBlocksToUpdate = 0;
                _flagsYAreasToUpdate = 0;
            }
        }

        /// <summary>
        /// Флаг блока который был изменён
        /// </summary>
        /// <param name="pos">локальные координаты xz 0..15, y 0..255</param>
        public void FlagBlockForUpdate(int x, int y, int z)
        {
            if (_players.Count > 0)
            {
                _flagsYAreasToUpdate |= (uint)(1 << (y >> 4));

                if (NumBlocksToUpdate < _CountMultyBlocks)
                {
                    int index = (y << 8) | (x << 4) | z;
                    for (int i = 0; i < NumBlocksToUpdate; i++)
                    {
                        if (_locationOfBlockChange[i] == index) return;
                    }
                    _locationOfBlockChange[NumBlocksToUpdate++] = index;
                }
            }
        }

        /// <summary>
        /// Флаг псевдочанка который был изменён
        /// </summary>
        public void FlagChunkForUpdate(int y)
        {
            if (_players.Count > 0)
            {
                NumBlocksToUpdate = _CountMultyBlocks;
                _flagsYAreasToUpdate |= (uint)(1 << y);
            }
        }

        public override string ToString() => CurrentChunkX + ":" + CurrentChunkY 
            + " P:" + _players.Count + " A:" + _anchors.Count;
    }
}
