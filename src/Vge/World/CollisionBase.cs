using System.Runtime.CompilerServices;
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
        /// Результат запроса, списка для поиска сущностей
        /// </summary>
        public readonly ListFast<EntityBase> ListEntity = new ListFast<EntityBase>();
        /// <summary>
        /// Результат запроса, списка для поиска блоков
        /// </summary>
        public readonly ListFast<AxisAlignedBB> ListBlock = new ListFast<AxisAlignedBB>();
        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        public readonly WorldBase World;
        /// <summary>
        /// Выбранный объект
        /// </summary>
        public readonly MovingObjectPosition MovingObject = new MovingObjectPosition();

        /// <summary>
        /// Количество блоков в чанке. NumberChunkSections * 16 - 1 (old COUNT_HEIGHT_BLOCK)
        /// </summary>
        private int _numberBlocks;

        public CollisionBase(WorldBase world) => World = world;

        /// <summary>
        /// Инициализация колизии, нужна чтоб задать высоту мира
        /// </summary>
        public void Init() => _numberBlocks = World.ChunkPr.Settings.NumberBlocks;

        /// <summary>
        /// Возвращает список статических (блоков) ограничивающих рамок, которые сталкиваются с aabb
        /// </summary>
        /// <param name="aabb">проверяемая рамка</param>
        public void StaticBoundingBoxes(AxisAlignedBB aabb)
        {
            ListBlock.Clear();
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
                ChunkBase chunk = World.GetChunk(minCx, minCz);
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
                                    ListBlock.AddRange(block.GetCollisionBoxesToList(new BlockPos(x, y, z), blockState.Met));
                                }
                            }
                            else
                            {
                                // Для колизи важно, если чанк не загружен, то блоки все с колизией, так-как начнём падать
                                ListBlock.Add(new AxisAlignedBB(x, y, z, x + 1, y + 1, z + 1));
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
                        ChunkBase chunk = World.GetChunk(xc, zc);
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
                                                    ListBlock.AddRange(block.GetCollisionBoxesToList(new BlockPos(x, y, z), blockState.Met));
                                                }
                                            }
                                            else
                                            {
                                                // Для колизи важно, если чанк не загружен, то блоки все с колизией, так-как начнём падать
                                                ListBlock.Add(new AxisAlignedBB(x, y, z, x + 1, y + 1, z + 1));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
                ChunkBase chunk = World.GetChunk(minCx, minCz);
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
                        ChunkBase chunk = World.GetChunk(xc, zc);
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
        /// Собрать список сущностей, которые могут сталкиваются с aabb из секторов чанка,
        /// </summary>
        /// <param name="aabb">проверяемая рамка</param>
        /// <param name="id">исключение ID сущности</param>
        public void EntityBoundingBoxesFromSector(AxisAlignedBB aabb, int id)
        {
            ListEntity.Clear();
            // TODO::2025-06-18 Добавляю во все стороны по два блока, чтоб найти в соседнем чанке сущность которая может выступать
            Vector3i min = aabb.MinInt() - 2;
            Vector3i max = aabb.MaxInt() + 2;

            int minCx = min.X >> 4;
            int minCz = min.Z >> 4;
            int maxCx = max.X >> 4;
            int maxCz = max.Z >> 4;
            int minCY = min.Y;
            if (minCY < 0) minCY = 0; else if (minCY > _numberBlocks) minCY = _numberBlocks;
            minCY = minCY >> 4;
            int maxCY = max.Y;
            if (maxCY < 0) maxCY = 0; else if (maxCY > _numberBlocks) maxCY = _numberBlocks;
            maxCY = maxCY >> 4;

            int xc, zc;
            for (xc = minCx; xc <= maxCx; xc++)
            {
                for (zc = minCz; zc <= maxCz; zc++)
                {
                    ChunkBase chunk = World.GetChunk(xc, zc);
                    // Не надо отрабатывать null, для этого есть отработка в статике
                    if (chunk != null)
                    {
                        chunk.FillInEntityBoundingBoxesFromSector(ListEntity, aabb, minCY, maxCY, id);
                    }
                }
            }
        }

        /// <summary>
        /// Имеется ли хоть одна сущность, которые могут сталкиваются с aabb из секторов чанка,
        /// </summary>
        /// <param name="aabb">проверяемая рамка</param>
        /// <param name="id">исключение ID сущности</param>
        public bool IsEntityBoundingBoxesFromSector(AxisAlignedBB aabb, int id)
        {
            // TODO::2025-06-18 Добавляю во все стороны по два блока, чтоб найти в соседнем чанке сущность которая может выступать
            Vector3i min = aabb.MinInt() - 2;
            Vector3i max = aabb.MaxInt() + 2;

            int minCx = min.X >> 4;
            int minCz = min.Z >> 4;
            int maxCx = max.X >> 4;
            int maxCz = max.Z >> 4;
            int minCY = min.Y;
            if (minCY < 0) minCY = 0; else if (minCY > _numberBlocks) minCY = _numberBlocks;
            minCY = minCY >> 4;
            int maxCY = max.Y;
            if (maxCY < 0) maxCY = 0; else if (maxCY > _numberBlocks) maxCY = _numberBlocks;
            maxCY = maxCY >> 4;

            int xc, zc;
            for (xc = minCx; xc <= maxCx; xc++)
            {
                for (zc = minCz; zc <= maxCz; zc++)
                {
                    ChunkBase chunk = World.GetChunk(xc, zc);
                    // Не надо отрабатывать null, для этого есть отработка в статике
                    if (chunk != null && chunk.IsEntityBoundingBoxesFromSector(aabb, minCY, maxCY, id))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Пересечения луча
        /// </summary>
        /// <param name="px">точка X от куда идёт лучь</param>
        /// <param name="py">точка Y от куда идёт лучь</param>
        /// <param name="pz">точка Z от куда идёт лучь</param>
        /// <param name="dir">вектор луча, нормализованный</param>
        /// <param name="maxDist">максимальная дистания</param>
        /// <param name="collidable">сталкивающийся</param>
        /// <param name="entityId">исключение ID сущности, он же пускает луч</param>
        /// <param name="isLiquid">ловим жидкость</param>
        /// <param name="isLight">игнорируем прозрачные блоки</param>
        public void RayCast(float px, float py, float pz,
            Vector3 dir, float maxDist, bool collidable, int entityId,
            bool isLiquid = false, bool isLight = false)
        {

            // Пересечения лучей с визуализируемой поверхностью для блока
            RayCastBlock(px, py, pz, dir, maxDist, collidable, isLiquid, isLight);

            Vector3 pos = new Vector3(px, py, pz);
            Vector3 to = MovingObject.IsBlock() ? MovingObject.RayHit : pos + dir * maxDist;

            // Собираем все близлижащий сущностей для дальнейше проверки
            // TODO::2025-06-19 Тут должна быть hitbox не для коллизии, а для атаки или взаимодействия!
            EntityBoundingBoxesFromSector(new AxisAlignedBB(pos, to), entityId);

            int count = ListEntity.Count;
            if (count > 0)
            {
                EntityBase entity;
                EntityBase entityHit = null;
                PointIntersection intersectionHit = new PointIntersection();
                PointIntersection intersection;
                float distance = MovingObject.IsBlock() ? Glm.Distance(pos, MovingObject.RayHit) : float.MaxValue;

                for (int i = 0; i < count; i++)
                {
                    entity = ListEntity[i];
                    // Проверка может ли сущность быть в проверке
                    if (entity.CanBeCollidedWith())
                    {
                        intersection = entity.Size.CalculateIntercept(pos, to);
                        if (intersection.Intersection)
                        {
                            // Имеется пересечение с сущностью
                            float f = Glm.Distance(pos, intersection.RayHit);
                            if (f < distance)
                            {
                                entityHit = entity;
                                intersectionHit = intersection;
                                distance = f;
                            }
                        }
                    }
                }
                ListEntity.ClearFull();

                if (entityHit != null)
                {
                    MovingObject.ObjectEntity(entityHit, intersectionHit);
                }
            }
        }


        /// <summary>
        /// Пересечения лучей с визуализируемой поверхностью для блока
        /// </summary>
        /// <param name="px">точка X от куда идёт лучь</param>
        /// <param name="py">точка Y от куда идёт лучь</param>
        /// <param name="pz">точка Z от куда идёт лучь</param>
        /// <param name="dir">вектор луча, нормализованный</param>
        /// <param name="maxDist">максимальная дистания</param>
        /// <param name="collidable">сталкивающийся</param>
        /// <param name="isLiquid">ловим жидкость</param>
        /// <param name="isLight">игнорируем прозрачные блоки</param>
        public void RayCastBlock(float px, float py, float pz,
            Vector3 dir, float maxDist, bool collidable,
            bool isLiquid = false, bool isLight = false)
        {
            MovingObject.Clear();

            float dx = dir.X;
            float dy = dir.Y;
            float dz = dir.Z;

            float t = 0.0f;
            int ix = Mth.Floor(px);
            int iy = Mth.Floor(py);
            int iz = Mth.Floor(pz);
            int stepx = (dx > 0.0f) ? 1 : -1;
            int stepy = (dy > 0.0f) ? 1 : -1;
            int stepz = (dz > 0.0f) ? 1 : -1;
            Pole sidex = (dx > 0.0f) ? Pole.West : Pole.East;
            Pole sidey = (dy > 0.0f) ? Pole.Down : Pole.Up;
            Pole sidez = (dz > 0.0f) ? Pole.North : Pole.South;

            float infinity = float.MaxValue;

            float txDelta = (dx == 0.0f) ? infinity : Mth.Abs(1.0f / dx);
            float tyDelta = (dy == 0.0f) ? infinity : Mth.Abs(1.0f / dy);
            float tzDelta = (dz == 0.0f) ? infinity : Mth.Abs(1.0f / dz);

            float xdist = (stepx > 0) ? (ix + 1 - px) : (px - ix);
            float ydist = (stepy > 0) ? (iy + 1 - py) : (py - iy);
            float zdist = (stepz > 0) ? (iz + 1 - pz) : (pz - iz);

            float txMax = (txDelta < infinity) ? txDelta * xdist : infinity;
            float tyMax = (tyDelta < infinity) ? tyDelta * ydist : infinity;
            float tzMax = (tzDelta < infinity) ? tzDelta * zdist : infinity;

            int steppedIndex = -1;

            bool liquid = false;
            BlockPos blockPosLiquid = new BlockPos();
            int idBlockLiquid = -1;

            int idBlock;
            BlockPos blockPos = new BlockPos();
            BlockState blockState;
            BlockBase block;
            Pole side = Pole.Up;
            Vector3i norm;
            Vector3 end;

            while (t <= maxDist)
            {
                blockPos.X = ix;
                blockPos.Y = iy;
                blockPos.Z = iz;
                blockState = World.GetBlockState(blockPos);
                block = blockState.GetBlock();
                idBlock = blockState.Id;

                if (isLiquid && !liquid && block.Liquid)// && !(block is BlockAbLiquidFlowing))
                {
                    liquid = true;
                    blockPosLiquid.X = blockPos.X;
                    blockPosLiquid.Y = blockPos.Y;
                    blockPosLiquid.Z = blockPos.Z;
                    idBlockLiquid = idBlock;
                }

                if ((isLight && block.IsNotTransparent) || (!isLight && ((!collidable) || (collidable && block.IsCollidable))
                    && block.CollisionRayTrace(blockPos, blockState.Met, px, py, pz, dir, maxDist)))
                {
                    end.X = px + t * dx;
                    end.Y = py + t * dy;
                    end.Z = pz + t * dz;

                    norm.X = norm.Y = norm.Z = 0;
                    if (steppedIndex == 0)
                    {
                        side = sidex;
                        norm.X = -stepx;
                    }
                    else if (steppedIndex == 1)
                    {
                        side = sidey;
                        norm.Y = -stepy;
                    }
                    else if (steppedIndex == 2)
                    {
                        side = sidez;
                        norm.Z = -stepz;
                    }
                    MovingObject.ObjectBlock(blockState, blockPos, side, end - blockPos.ToVector3(), norm, end);
                    break;
                }
                if (txMax < tyMax)
                {
                    if (txMax < tzMax)
                    {
                        ix += stepx;
                        t = txMax;
                        txMax += txDelta;
                        steppedIndex = 0;
                    }
                    else
                    {
                        iz += stepz;
                        t = tzMax;
                        tzMax += tzDelta;
                        steppedIndex = 2;
                    }
                }
                else
                {
                    if (tyMax < tzMax)
                    {
                        iy += stepy;
                        t = tyMax;
                        tyMax += tyDelta;
                        steppedIndex = 1;
                    }
                    else
                    {
                        iz += stepz;
                        t = tzMax;
                        tzMax += tzDelta;
                        steppedIndex = 2;
                    }
                }
            }

            if (isLiquid)
            {
                MovingObject.SetLiquid(idBlockLiquid, blockPosLiquid);
            }
        }

        /// <summary>
        /// Имеется ли хоть одно статическое (блок) или сущности столкновения с aabb
        /// </summary>
        /// <param name="aabb">проверяемая рамка</param>
        /// <param name="id">исключение ID сущности</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBoundingBoxes(AxisAlignedBB aabb, int id)
            => IsStaticBoundingBoxes(aabb) || IsEntityBoundingBoxesFromSector(aabb, id);

        #region Old
        /*
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
        */
        #endregion
    }
}
