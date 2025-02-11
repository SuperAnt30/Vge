using System.Collections.Generic;
using System.Diagnostics;
using Vge.Entity;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.World
{
    /// <summary>
    /// Базовый класс проверки колизии
    /// </summary>
    public class CollisionBase
    {
        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        private readonly WorldBase _world;
        /// <summary>
        /// Количество блоков в чанке. NumberChunkSections * 16 - 1 (old COUNT_HEIGHT_BLOCK)
        /// </summary>
        private int _numberBlocks;

        public CollisionBase(WorldBase world) => _world = world;

        /// <summary>
        /// Инициализация колизии, нужна чтоб задать высоту мира
        /// </summary>
        public void Init() => _numberBlocks = _world.ChunkPr.Settings.NumberBlocks;

        

        /// <summary>
        /// Возвращает список статических (блоков) ограничивающих рамок, которые сталкиваются с aabb
        /// </summary>
        /// <param name="aabb">проверяемая рамка</param>
        public List<AxisAlignedBB> GetStaticBoundingBoxes(AxisAlignedBB aabb)
        {
            List<AxisAlignedBB> list = new List<AxisAlignedBB>();
            Vector3i min = aabb.MinInt();
            Vector3i max = aabb.MaxInt();
                
            // Оптимизация 1
            int minCx = min.X >> 4;
            int minCz = min.Z >> 4;
            int maxCx = max.X >> 4;
            int maxCz = max.Z >> 4;
            int minY = min.Y;
            if (minY < 0) minY = 0; else if (minY > _numberBlocks) minY = _numberBlocks;
            int maxY = max.Y;
            if (maxY < 0) maxY = 0; else if (maxY > _numberBlocks) maxY = _numberBlocks;

            BlockState blockState;
            BlockBase block;
            int xb, zb, x, y, z;

            if (minCx == maxCx && minCz == maxCz)
            {
                ChunkBase chunk = _world.GetChunk(minCx, minCz);
                for (x = min.X; x <= max.X; x++)
                {
                    xb = x & 15;
                    for (z = min.Z; z <= max.Z; z++)
                    {
                        zb = z & 15;
                        for (y = minY; y <= maxY; y++)
                        {
                            if (chunk != null)
                            {
                                // делаем без колизии если чанк загружен, чтоб можно было в пустых псевдо чанках двигаться
                                blockState = chunk.GetBlockStateNotCheckLight(xb, y, zb);
                                block = blockState.GetBlock();
                                if (block.IsCollidable)
                                {
                                    list.AddRange(block.GetCollisionBoxesToList(new BlockPos(x, y, z), blockState.Met));
                                }
                            }
                            else
                            {
                                // Для колизи важно, если чанк не загружен, то блоки все с колизией, так-как начнём падать
                                list.Add(new AxisAlignedBB(x, y, z, x + 1, y + 1, z + 1));
                            }
                        }
                    }
                }
            }
            else
            {
                int xc, zc;
                for (xc = minCx; xc <= maxCx; xc++)
                {
                    for (zc = minCz; zc <= maxCz; zc++)
                    {
                        ChunkBase chunk = _world.GetChunk(xc, zc);
                        for (x = min.X; x <= max.X; x++)
                        {
                            if (x >> 4 == xc)
                            {
                                xb = x & 15;
                                for (z = min.Z; z <= max.Z; z++)
                                {
                                    if (z >> 4 == zc)
                                    {
                                        zb = z & 15;
                                        for (y = minY; y <= maxY; y++)
                                        {
                                            if (chunk != null)
                                            {
                                                // делаем без колизии если чанк загружен, чтоб можно было в пустых псевдо чанках двигаться
                                                blockState = chunk.GetBlockStateNotCheckLight(xb, y, zb);
                                                block = blockState.GetBlock();
                                                if (block.IsCollidable)
                                                {
                                                    list.AddRange(block.GetCollisionBoxesToList(new BlockPos(x, y, z), blockState.Met));
                                                }
                                            }
                                            else
                                            {
                                                // Для колизи важно, если чанк не загружен, то блоки все с колизией, так-как начнём падать
                                                list.Add(new AxisAlignedBB(x, y, z, x + 1, y + 1, z + 1));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Имеется ли хоть одна статическая (блок) ограничивающая рамка столкновения с aabb
        /// </summary>
        /// <param name="aabb">проверяемая рамка</param>
        public bool IsStaticBoundingBoxes(AxisAlignedBB aabb)
        {
            Vector3i min = aabb.MinInt();
            Vector3i max = aabb.MaxInt();

            int minCx = min.X >> 4;
            int minCz = min.Z >> 4;
            int maxCx = max.X >> 4;
            int maxCz = max.Z >> 4;
            int minY = min.Y;
            if (minY < 0) minY = 0; else if (minY > _numberBlocks) minY = _numberBlocks;
            int maxY = max.Y;
            if (maxY < 0) maxY = 0; else if (maxY > _numberBlocks) maxY = _numberBlocks;

            int xb, zb, x, y, z;

            if (minCx == maxCx && minCz == maxCz)
            {
                ChunkBase chunk = _world.GetChunk(minCx, minCz);
                if (chunk != null)
                {
                    for (x = min.X; x <= max.X; x++)
                    {
                        xb = x & 15;
                        for (z = min.Z; z <= max.Z; z++)
                        {
                            zb = z & 15;
                            for (y = minY; y <= maxY; y++)
                            {
                                // делаем без колизии если чанк загружен, чтоб можно было в пустых псевдо чанках двигаться
                                if (chunk.GetBlockStateNotCheckLight(xb, y, zb).GetBlock().IsCollidable)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Для колизи важно, если чанк не загружен, то блоки все с колизией, так-как начнём падать
                    return true;
                }
            }
            else
            {
                int xc, zc;
                for (xc = minCx; xc <= maxCx; xc++)
                {
                    for (zc = minCz; zc <= maxCz; zc++)
                    {
                        ChunkBase chunk = _world.GetChunk(xc, zc);
                        if (chunk != null)
                        {
                            for (x = min.X; x <= max.X; x++)
                            {
                                if (x >> 4 == xc)
                                {
                                    xb = x & 15;
                                    for (z = min.Z; z <= max.Z; z++)
                                    {
                                        if (z >> 4 == zc)
                                        {
                                            zb = z & 15;
                                            for (y = minY; y <= maxY; y++)
                                            {
                                                // делаем без колизии если чанк загружен, чтоб можно было в пустых псевдо чанках двигаться
                                                if (chunk.GetBlockStateNotCheckLight(xb, y, zb).GetBlock().IsCollidable)
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Для колизи важно, если чанк не загружен, то блоки все с колизией, так-как начнём падать
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Возвращает список сущностей, которые могут сталкиваются с aabb из секторов чанка,
        /// </summary>
        /// <param name="aabb">проверяемая рамка</param>
        /// <param name="id">исключение ID сущности</param>
        public List<EntityBase> GetEntityBoundingBoxesFromSector(AxisAlignedBB aabb, int id)
        {
            List<EntityBase> list = new List<EntityBase>();
            Vector3i min = aabb.MinInt();
            Vector3i max = aabb.MaxInt();

            int minCx = min.X >> 4;
            int minCz = min.Z >> 4;
            int maxCx = max.X >> 4;
            int maxCz = max.Z >> 4;
            int minY = min.Y;
            if (minY < 0) minY = 0; else if (minY > _numberBlocks) minY = _numberBlocks;
            minY = minY >> 4;
            int maxY = max.Y;
            if (maxY < 0) maxY = 0; else if (maxY > _numberBlocks) maxY = _numberBlocks;
            maxY = maxY >> 4;

            int xc, zc;
            for (xc = minCx; xc <= maxCx; xc++)
            {
                for (zc = minCz; zc <= maxCz; zc++)
                {
                    ChunkBase chunk = _world.GetChunk(xc, zc);
                    // Не надо отрабатывать null, для этого есть отработка в статике
                    if (chunk != null)
                    {
                        chunk.FillInEntityBoundingBoxesFromSector(list, minY, maxY, id);
                    }
                }
            }

            return list;
        }

        #region Old

        /// <summary>
        /// Возвращает список сущностей, которые сталкиваются с aabb,
        /// </summary>
        /// <param name="aabb">проверяемая рамка</param>
        /// <param name="id">исключение ID сущности</param>
        public List<EntityBase> GetEntityBoundingBoxes(AxisAlignedBB aabb, int id)
        {
            List<EntityBase> list = new List<EntityBase>();
            AxisAlignedBB aabbRegion = aabb.Expand(2, 2, 2);
            Vector3i min = aabbRegion.MinInt();
            Vector3i max = aabbRegion.MaxInt();

            int minCx = min.X >> 4;
            int minCz = min.Z >> 4;
            int maxCx = max.X >> 4;
            int maxCz = max.Z >> 4;
            int minY = min.Y;
            if (minY < 0) minY = 0; else if (minY > _numberBlocks) minY = _numberBlocks;
            minY = minY >> 4;
            int maxY = max.Y;
            if (maxY < 0) maxY = 0; else if (maxY > _numberBlocks) maxY = _numberBlocks;
            maxY = maxY >> 4;

            int xc, zc;
            for (xc = minCx; xc <= maxCx; xc++)
            {
                for (zc = minCz; zc <= maxCz; zc++)
                {
                    ChunkBase chunk = _world.GetChunk(xc, zc);
                    // Не надо отрабатывать null, для этого есть отработка в статике
                    if (chunk != null)
                    {
                        chunk.FillInEntityBoundingBoxes(list, aabb, minY, maxY, id);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Получить блок в глобальной координате
        /// </summary>
        private BlockState _GetBlockState(int x, int y, int z)
        {
            if (y >= 0 && y <= _numberBlocks)
            {
                ChunkBase chunk = _world.GetChunk(x >> 4, z >> 4);
                if (chunk != null)
                {
                    // делаем без колизии если чанк загружен, чтоб можно было в пустых псевдо чанках двигаться
                    return chunk.GetBlockStateNotCheckLight(x & 15, y, z & 15);
                }
                // Для колизи важно, если чанк не загружен, то блоки все с колизией, так-как начнём падать
                return new BlockState().Empty();
            }
            return new BlockState();
        }

        /// <summary>
        /// Обработка колизии блока, особенно важен когда блок не цельный
        /// </summary>
        /// <param name="xyz">координата блока</param>
        /// <param name="aabb">проверяемая рамка</param>
        /// <returns>true - пересечение имеется</returns>
        private bool _BlockCollision(int x, int y, int z, AxisAlignedBB aabb)
        {
            BlockState blockState = _GetBlockState(x, y, z);
            BlockBase block = blockState.GetBlock();
            if (block.IsCollidable || blockState.IsEmpty())
            {
                // Цельный блок на коллизию
                if (block.FullBlock) return true;
                // Выбираем часть блока
                foreach (AxisAlignedBB aabbBlock in block.GetCollisionBoxesToList(new BlockPos(x, y, z), blockState.Met))
                {
                    if (aabbBlock.IntersectsWith(aabb)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Проверяем коллизию тела c блоками
        /// </summary>
        /// <param name="entity">Сущность проверки</param>
        /// <param name="posX">Глобальная позиция Х</param>
        /// <param name="posY">Глобальная позиция Y</param>
        /// <param name="posZ">Глобальная позиция Z</param>
        public bool IsCollisionBody(EntityBase entity, float posX, float posY, float posZ)
        {
            AxisAlignedBB aabb = entity.GetBoundingBox(posX, posY, posZ).Expand(-.01f);
            Vector3i min = aabb.MinInt();
            Vector3i max = aabb.MaxInt();

            for (int y = min.Y; y <= max.Y; y++)
            {
                if (y >= 0 && y <= _numberBlocks)
                {
                    for (int x = min.X; x <= max.X; x++)
                    {
                        for (int z = min.Z; z <= max.Z; z++)
                        {
                            if (_BlockCollision(x, y, z, aabb)) return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
