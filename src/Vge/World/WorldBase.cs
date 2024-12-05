using Vge.Util;
using Vge.World.Block;
using Vge.World.Chunk;
using Vge.World.Light;
using WinGL.Util;

namespace Vge.World
{
    /// <summary>
    /// Абстрактный объект мира
    /// </summary>
    public abstract class WorldBase
    {
        /// <summary>
        /// Это значение true для клиентских миров и false для серверных миров.
        /// </summary>
        public bool IsRemote { get; protected set; }
        /// <summary>
        /// Объект генератора случайных чисел
        /// </summary>
        public Rand Rnd { get; protected set; }
        /// <summary>
        /// Объект сыщик
        /// </summary>
        public Profiler Filer { get; protected set; }
        /// <summary>
        /// Посредник чанков
        /// </summary>
        public ChunkProvider ChunkPr { get; protected set; }

        /// <summary>
        /// Объект обработки освещения для мира
        /// </summary>
        public readonly WorldLight Light;

        public WorldBase(int numberBlocksToLight) => Light = new WorldLight(this, numberBlocksToLight);

        /// <summary>
        /// Получить время в тактах объекта Stopwatch с момента запуска проекта
        /// </summary>
        public virtual long ElapsedTicks() => 0;

        #region Chunk

        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        public ChunkBase GetChunk(int chunkPosX, int chunkPosY) 
            => ChunkPr.GetChunk(chunkPosX, chunkPosY);
        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        public ChunkBase GetChunk(Vector2i chunkPos)
            => ChunkPr.GetChunk(chunkPos);

        #endregion

        #region Block

        /// <summary>
        /// Получить блок данных
        /// </summary>
        public BlockState GetBlockState(BlockPos blockPos)
        {
            if (blockPos.IsValid(ChunkPr.Settings))
            {
                ChunkBase chunk = GetChunk(blockPos.GetPositionChunk());
                if (chunk != null)
                {
                    return chunk.GetBlockStateNotCheck(blockPos.X & 15, blockPos.Y, blockPos.Z & 15);
                }
            }
            return new BlockState().Empty();
        }

        /// <summary>
        /// Задать блок неба, с флагом по умолчанию 14 (уведомление соседей, modifyRender, modifySave)
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="flag">флаг, 1 частички старого блока, 2 уведомление соседей, 4 modifyRender, 8 modifySave, 16 sound break</param>
        public bool SetBlockToAir(BlockPos blockPos, int flag = 14) 
            => SetBlockState(blockPos, new BlockState(0), flag);

        /// <summary>
        /// Сменить блок
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="blockState">данные блока</param>
        /// <param name="flag">флаг, 1 частички старого блока, 2 уведомление соседей, 4 modifyRender, 8 modifySave, 16 sound break</param>
        /// <returns>true смена была</returns>
        public virtual bool SetBlockState(BlockPos blockPos, BlockState blockState, int flag)
        {
            if (!blockPos.IsValid(ChunkPr.Settings)) return false;

            ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
            if (chunk == null) return false;

            BlockState blockStateTrue = chunk.SetBlockState(blockPos, blockState, (flag & 8) != 0, (flag & 4) != 0, (flag & 16) != 0);
            if (blockStateTrue.IsEmpty()) return false;

            //BlockState blockStateTrue = chunk.SetBlockState(pos.X, pos.Y, pos.Z, blockState, (flag & 8) != 0, (flag & 4) != 0, (flag & 16) != 0);
            //if (blockStateTrue.IsEmpty()) return false;

            //if (!IsRemote)
            //{
            //    // Частички блока, только на сервере чтоб всем разослать
            //    if ((flag & 1) != 0) ParticleDiggingBlock(blockStateTrue.GetBlock(), blockPos, 50);
            //}
            //// Уведомление соседей и на сервере и на клиенте
            //if ((flag & 2) != 0) NotifyNeighborsOfStateChange(blockPos, blockState.GetBlock());

            return true;
        }

        #endregion

        #region Mark

        /// <summary>
        /// Отметить блок для обновления
        /// </summary>
        public virtual void MarkBlockForUpdate(int x, int y, int z) { }
        /// <summary>
        /// Отметить блоки для обновления
        /// </summary>
        public virtual void MarkBlockRangeForUpdate(int x0, int y0, int z0, int x1, int y1, int z1) { }
        /// <summary>
        /// Отметить блоки для изминения
        /// </summary>
        public virtual void MarkBlockRangeForModified(int x0, int z0, int x1, int z1) { }

        #endregion

        /// <summary>
        /// Пересечения лучей с визуализируемой поверхностью для блока
        /// </summary>
        /// <param name="pos">точка от куда идёт лучь</param>
        /// <param name="dir">вектор луча</param>
        /// <param name="maxDist">максимальная дистания</param>
        /// <param name="collidable">сталкивающийся</param>
        /// <param name="isLiquid">ловим жидкость</param>
        /// <param name="isLight">игнорируем прозрачные блоки</param>
        public MovingObjectPosition RayCastBlock(Vector3 pos, Vector3 dir, float maxDist, bool collidable,
            bool isLiquid = false, bool isLight = false)
        {
            float px = pos.X;
            float py = pos.Y;
            float pz = pos.Z;

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
            MovingObjectPosition moving = new MovingObjectPosition();

            while (t <= maxDist)
            {
                blockPos.X = ix;
                blockPos.Y = iy;
                blockPos.Z = iz;
                blockState = GetBlockState(blockPos);
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
                    && block.CollisionRayTrace(blockPos, blockState.Met, pos, dir, maxDist)))
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
                    moving = new MovingObjectPosition(blockState, blockPos, side, end - blockPos.ToVector3(), norm, end);
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
                moving.SetLiquid(idBlockLiquid, blockPosLiquid);
            }
            return moving;
        }

        /// <summary>
        /// Для отладки
        /// </summary>
        public virtual void DebugString(string logMessage, params object[] args) { }
    }
}
