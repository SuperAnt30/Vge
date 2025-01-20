using System.Collections.Generic;
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
        /// Возвращает список ограничивающих рамок, которые сталкиваются с aabb,
        /// /*за исключением переданного столкновения сущности.*/
        /// </summary>
        /// <param name="aabb">проверяемая рамка</param>
        public List<AxisAlignedBB> GetCollidingBoundingBoxes(AxisAlignedBB aabb, int id)
        {
            List<AxisAlignedBB> list = new List<AxisAlignedBB>();
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
                            BlockState blockState = _GetBlockState(x, y, z);
                            BlockBase block = blockState.GetBlock();
                            if (block.IsCollidable || blockState.IsEmpty())
                            {
                                list.AddRange(block.GetCollisionBoxesToList(new BlockPos(x, y, z), blockState.Met));
                            }
                        }
                    }
                }
            }

            // Добавим сущностей
            // TODO::2025-01-20 сущности колизии выборка из чанков
            int count = _world.LoadedEntityList.Count;
            EntityBase entity;
            for (int i = 0; i < count; i++)
            {
                entity = _world.LoadedEntityList.GetAt(i) as EntityBase;
                if (entity.Id != id)
                {
                    list.Add(entity.GetBoundingBox());
                }
            }

            return list;
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
    }
}
