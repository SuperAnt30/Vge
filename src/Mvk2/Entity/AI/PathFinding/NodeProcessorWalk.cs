
using Mvk2.World.Block;
using Vge.Entity;
using Vge.Entity.AI.PathFinding;
using Vge.World;
using Vge.World.Block;
using WinGL.Util;

namespace Mvk2.Entity.AI.PathFinding
{
    /// <summary>
    /// Процессор узла ходьбы
    /// </summary>
    public class NodeProcessorWalk : NodeProcessor
    {
        /// <summary>
        /// Может плавать
        /// </summary>
        public bool CanSwim = false;
        /// <summary>
        /// Избегает воды 
        /// </summary>
        public bool AvoidsWater = true;
        /// <summary>
        /// Избегает лавы и огня
        /// </summary>
        public bool AvoidsLavaOrFire = true;
        /// <summary>
        /// Ходить через дверь
        /// </summary>
        public bool ThroughDdoor = false;
        
        /// <summary>
        /// Следует избегать воды
        /// </summary>
        private bool _shouldAvoidWater = true;

        /// <summary>
        /// Инициализация
        /// </summary>
        public override void InitProcessor(WorldServer world, EntityMob entity)
        {
            base.InitProcessor(world, entity);
            _shouldAvoidWater = AvoidsWater;
        }

        /// <summary>
        /// Этот метод вызывается после обработки всех узлов и создания PathEntity
        /// </summary>
        public override void PostProcess()
        {
            base.PostProcess();
            AvoidsWater = _shouldAvoidWater;
        }

        /// <summary>
        /// Возвращает точку пути куда надо придти
        /// </summary>
        public override PathPoint GetPathPointToCoords(EntityMob entity, float x, float y, float z)
            => _pathPointEnd = OpenPoint(Mth.Floor(x), Mth.Floor(y), Mth.Floor(z));

        /// <summary>
        /// Возвращает точку пути от куда стартуем
        /// </summary>
        public override PathPoint GetPathPointTo(EntityMob entity)
        {
            int y;

            if (CanSwim && entity.PresenceBlocks.IsInWater)
            {
                // если в воде и избегает воды, выплываем вверх, и помечаем, что не избегаем воды
                y = (int)entity.PosY;

                for (BlockBase block = _world.GetBlockState(new BlockPos(Mth.Floor(entity.PosX), y, Mth.Floor(entity.PosZ))).GetBlock();
                    block.Material.IndexMaterial == (int)EnumMaterial.Water;
                    block = _world.GetBlockState(new BlockPos(Mth.Floor(entity.PosX), y, Mth.Floor(entity.PosZ))).GetBlock())
                {
                    ++y;
                }

                AvoidsWater = false;
            }
            else
            {
                y = Mth.Floor(entity.PosY + .5f);
            }

            return OpenPoint(Mth.Floor(entity.PosX), y, Mth.Floor(entity.PosZ));
        }

        /// <summary>
        /// Найти параметры пути
        /// </summary>
        public override int FindPathOptions(PathPoint[] points, EntityMob entity, PathPoint pointBegin, PathPoint pointEnd, float distance)
        {
            int index = 0;
            byte up = 0;

            // Проверяем состояние, можем ли мы залесть на один блок при необходимости
            if (!_CheckBlocksCollision(pointBegin.CoordX, pointBegin.CoordY + 1, pointBegin.CoordZ, true))
            {
                up = 1;
            }

            // Проверяем возможность перемещения в любую сторону
            PathPoint pointSouth = _GetSafePoint(entity, pointBegin.CoordX, pointBegin.CoordY, pointBegin.CoordZ, 0, 1, up);
            PathPoint pointWest = _GetSafePoint(entity, pointBegin.CoordX, pointBegin.CoordY, pointBegin.CoordZ, -1, 0, up);
            PathPoint pointEast = _GetSafePoint(entity, pointBegin.CoordX, pointBegin.CoordY, pointBegin.CoordZ, 1, 0, up);
            PathPoint pointNorth = _GetSafePoint(entity, pointBegin.CoordX, pointBegin.CoordY, pointBegin.CoordZ, 0, -1, up);

            PathPoint pointSouthWest = _GetSafePoint(entity, pointBegin.CoordX, pointBegin.CoordY, pointBegin.CoordZ, -1, 1, up);
            PathPoint pointSouthEast = _GetSafePoint(entity, pointBegin.CoordX, pointBegin.CoordY, pointBegin.CoordZ, 1, 1, up);
            PathPoint pointNorthWest = _GetSafePoint(entity, pointBegin.CoordX, pointBegin.CoordY, pointBegin.CoordZ, -1, -1, up);
            PathPoint pointNorthEast = _GetSafePoint(entity, pointBegin.CoordX, pointBegin.CoordY, pointBegin.CoordZ, 1, -1, up);

            if (pointSouth != null && !pointSouth.Visited && pointSouth.DistanceTo(pointEnd) < distance) points[index++] = pointSouth;
            if (pointWest != null && !pointWest.Visited && pointWest.DistanceTo(pointEnd) < distance) points[index++] = pointWest;
            if (pointEast != null && !pointEast.Visited && pointEast.DistanceTo(pointEnd) < distance) points[index++] = pointEast;
            if (pointNorth != null && !pointNorth.Visited && pointNorth.DistanceTo(pointEnd) < distance) points[index++] = pointNorth;

            if (pointSouthWest != null && !pointSouthWest.Visited && pointSouthWest.DistanceTo(pointEnd) < distance) points[index++] = pointSouthWest;
            if (pointSouthEast != null && !pointSouthEast.Visited && pointSouthEast.DistanceTo(pointEnd) < distance) points[index++] = pointSouthEast;
            if (pointNorthWest != null && !pointNorthWest.Visited && pointNorthWest.DistanceTo(pointEnd) < distance) points[index++] = pointNorthWest;
            if (pointNorthEast != null && !pointNorthEast.Visited && pointNorthEast.DistanceTo(pointEnd) < distance) points[index++] = pointNorthEast;

            return index;
        }

        /// <summary>
        /// Возвращает точку, в которую сущность может безопасно переместиться
        /// </summary>
        private PathPoint _GetSafePoint(EntityMob entity, int xCoord, int y, int zCoord, int xBias, int zBias, int up)
        {
            PathPoint point = null;

            int x = xCoord + xBias;
            int z = zCoord + zBias;

            // Проверяем тикущее состояние
            if (!_CheckBlocksCollision(x, y, z, true))
            {
                point = OpenPoint(x, y, z);
            }

            // Проверяем возможность запрыгнуть если есть возможность up == 1
            if (point == null && up == 1 && !_CheckBlocksCollision(x, y + up, z, false))
            {
                point = OpenPoint(x, y + up, z);
                y += up;
            }

            if (point != null)
            {
                // Всё-таки можно находится, надо проверить, что под нагами
                int height = 0;
                int i;

                for (i = 0; y > 0; point = OpenPoint(x, y, z))
                {
                    // Проверяем что под ногами
                    i = _CheckRowBlocks(x, y - 1, z, _sizeXZ);

                    if ((AvoidsWater && i == -1) || (AvoidsLavaOrFire && i == -3) || i == -2)
                    {
                        // Если вода, если избегаем воду или любая опастность (лава, огонь, нефть)
                        return null;
                    }

                    if (i != 1)
                    {
                        // Нет столкновения
                        break;
                    }

                    if (height++ >= entity.MaxFallHeight)
                    {
                        return null;
                    }

                    --y;

                    if (y <= 0)
                    {
                        // если пытаемся падать в пропость
                        return null;
                    }
                }
            }

            return point;
        }

        /// <summary>
        /// Проверьте блоки тела, для коллизии, возвращаем:
        /// true - столкновение, false - можно перемещаться
        /// </summary>
        /// <param name="isCollision">true - чек по колизии, false - чек по IsPassable</param>
        private bool _CheckBlocksCollision(int posX, int posY, int posZ, bool isCollision)
        {
            BlockState blockState;
            BlockBase block;
            MaterialBase material;
            EnumMaterial eMaterial;

            for (int x = posX; x < posX + _sizeXZ; ++x)
            {
                for (int y = posY; y < posY + _sizeY; ++y)
                {
                    for (int z = posZ; z < posZ + _sizeXZ; ++z)
                    {
                        blockState = _world.GetBlockState(new BlockPos(x, y, z));
                        if (!blockState.IsAir()) // Не воздух
                        {
                            block = blockState.GetBlock();
                            material = block.Material;
                            eMaterial = (EnumMaterial)material.IndexMaterial;

                            // TODO::2024-02-14 Зачем block.IsCollidable, вроде неиспользуется.
                            // if ((isCollision && block.IsCollidable) || (!isCollision && !block.IsPassable(blockState.met))) 
                            if (((ThroughDdoor && eMaterial != EnumMaterial.Door) || !ThroughDdoor)
                                && !block.Passable)
                            {
                                // столкновении с любым сплошным блоком и дверь если надо
                                return true;
                            }
                            if (eMaterial == EnumMaterial.Water && AvoidsWater)
                            {
                                // столкновении с водой (если избегает воды)
                                return true;
                            }
                            if (AvoidsLavaOrFire && material.Ignites)
                            {
                                // столкновении с лавой или огнём
                                return true;
                            }
                            if (eMaterial == EnumMaterial.Oil)
                            {
                                // столкновении с нефтью
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Проверьте блоки ряда снизу, чтоб можно было упасть, возвращаем:
        /// 1 -  нет столкновений для перемещения по блокам,
        /// 0 -  столкновении с любым сплошным блоком, можно ходить, !приоритет 
        /// -1 - столкновении с водой, 
        /// -2 - столкновении с нефтью,
        /// -3 - столкновении с лавой или огнём
        /// </summary>
        private int _CheckRowBlocks(int posX, int posY, int posZ, int size)
        {
            BlockState blockState;
            BlockBase block;
            MaterialBase material;
            EnumMaterial eMaterial;
            // имеется блок воды
            bool isWater = false;
            // имеется блок нефти
            bool isOil = false;
            // имеется блок лавы или огня
            bool isLavaOrFile = false;

            for (int x = posX; x < posX + size; ++x)
            {
                for (int z = posZ; z < posZ + size; ++z)
                {
                    blockState = _world.GetBlockState(new BlockPos(x, posY, z));
                    if (!blockState.IsAir()) // Не воздух
                    {
                        block = blockState.GetBlock();
                        material = block.Material;
                        eMaterial = (EnumMaterial)material.IndexMaterial;

                        if (eMaterial == EnumMaterial.Water)
                        {
                            // столкновении с водой
                            isWater = true;
                        }
                        else if (eMaterial == EnumMaterial.Oil)
                        {
                            // столкновении с нефтью
                            isOil = true;
                        }
                        else if (material.Ignites)
                        {
                            // столкновении с лавой или огнём
                            isLavaOrFile = true;
                        }
                        else if (!block.Passable)
                        {
                            // Можно ходить
                            return 0;
                        }
                    }
                }
            }

            if (isLavaOrFile) return -3;
            if (isOil) return -2;
            if (isWater) return -1;
            return 1;
        }
    }
}
