using Vge.Util;
using Vge.World;
using Vge.World.Chunk;

namespace Vge.Management
{
    public class TestAnchor : IAnchor
    {
        public bool IsActive => true;

        public byte ActiveRadius => 2;

        public bool IsPlayer => false;

        public byte OverviewChunk => 2;

        public byte OverviewChunkPrev => 2;

        public int ChunkPositionX { get; private set; }

        public int ChunkPositionZ { get; private set; }

        public int ChunkPosManagedX { get; private set; }

        public int ChunkPosManagedZ { get; private set; }

        public void AddChunk(int chunkPosX, int chunkPosY)
            => _loadingChunks.Add(new ChunkPosition(chunkPosX, chunkPosY));

        public bool IsAnOffsetNecessary() 
            => ChunkPositionX != ChunkPosManagedX || ChunkPositionZ != ChunkPosManagedZ;

        public bool IsChangeOverview() => false;

        public void MountedMovedAnchor()
        {
            ChunkPosManagedX = ChunkPositionX;
            ChunkPosManagedZ = ChunkPositionZ;
            _FilterChunkLoadQueueRevers();
        }

        public void RemoveChunk(int chunkPosX, int chunkPosY)
            => _loadingChunks.Remove(chunkPosX, chunkPosY);

        public bool CheckLoadingChunks() => _loadingChunksSort.Count > 0;

        public ulong ReturnChunkForLoading()
        {
            ulong index = _loadingChunksSort.GetLast();
            _loadingChunksSort.RemoveLast();
            int x = Conv.IndexToChunkX(index);
            int y = Conv.IndexToChunkY(index);
            _loadingChunks.Remove(x, y);
            return index;
        }

        private readonly WorldServer _worldServer;

        public TestAnchor(WorldServer worldServer)
        {
            _worldServer = worldServer;
        }

        int i = 10;
        int i2 = 0;

        public void Update()
        {
            i++;

            if (i > 10)
            {
                i2++;
                i = 0;
                if (i2 < 10)
                {
                    ChunkPositionX += 1;
                    _worldServer.Fragment.UpdateMountedMovingAnchor(this);
                }
                else
                {
                    ChunkPositionX -= 1;
                    _worldServer.Fragment.UpdateMountedMovingAnchor(this);
                }
                if (i2 >= 20)
                {
                    i2 = 0;
                }
            }
        }

        /// <summary>
        /// Список координат чанков для загрузки, формируется по дистанции к игроку.
        /// Список пополняется при перемещении и уменьшается при проверке, что чанк загружен.
        /// Когда все загружены должно быть 0.
        /// </summary>
        private ListFast<ulong> _loadingChunksSort = new ListFast<ulong>();
        /// <summary>
        /// Список всех ChunkPosition которые сервер должен загрузить возле игрока,
        /// Корректируется от перемещения, используется до сортировки.
        /// После сортирует в _loadingChunksSort.
        /// Когда все загружены должно быть 0
        /// </summary>
        private readonly MapChunk _loadingChunks = new MapChunk();

        /// <summary>
        /// Фильтрация очереди загрузки фрагментов от центра к краю (реверс)
        /// </summary>
        private void _FilterChunkLoadQueueRevers()
        {
            // Реверс спирали
            _loadingChunksSort.Clear();
            int x, y, i, i2, i3;
            bool isClient = false;
            int overviewClient = OverviewChunk;
            int overview = FragmentManager.GetActiveRadiusAddServer(OverviewChunk, this);

            for (i = overview; i > 0; i--)
            {
                if (!isClient && i <= overviewClient)
                {
                    isClient = true;
                }
                i2 = ChunkPosManagedX - i;
                i3 = ChunkPosManagedX + i;
                y = i + ChunkPosManagedZ;
                for (x = i2; x <= i3; x++)
                {
                    if (_loadingChunks.Contains(x, y)) _loadingChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                }
                y = ChunkPosManagedZ - i;
                for (x = i2; x <= i3; x++)
                {
                    if (_loadingChunks.Contains(x, y)) _loadingChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                }
                i2 = ChunkPosManagedZ - i + 1;
                i3 = ChunkPosManagedZ + i - 1;
                x = i + ChunkPosManagedX;
                for (y = i2; y <= i3; y++)
                {
                    if (_loadingChunks.Contains(x, y)) _loadingChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                }
                x = ChunkPosManagedX - i;
                for (y = i2; y <= i3; y++)
                {
                    if (_loadingChunks.Contains(x, y)) _loadingChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                }
            }
            // Позиция где стоит игрок
            if (_loadingChunks.Contains(ChunkPosManagedX, ChunkPosManagedZ))
            {
                _loadingChunksSort.Add(Conv.ChunkXyToIndex(ChunkPosManagedX, ChunkPosManagedZ));
            }
        }
    }
}
