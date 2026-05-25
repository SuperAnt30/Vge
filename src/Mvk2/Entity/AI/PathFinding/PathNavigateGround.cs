using Mvk2.World.Block;
using System.Runtime.CompilerServices;
using Vge.Entity;
using Vge.Entity.AI.PathFinding;
using Vge.World;
using Vge.World.Block;
using WinGL.Util;

namespace Mvk2.Entity.AI.PathFinding
{
    /// <summary>
    /// Путь навигации по земле
    /// </summary>
    public class PathNavigateGround : PathNavigate
    {
        /// <summary>
        /// Ограничитель солнца, если true - то сущность будет избегать прямые блоки от солнца
        /// </summary>
        public bool restrictSun = false;

        private NodeProcessorWalk nodeProcessor;

        /// <summary>
        /// Путь навигации по земле
        /// </summary>
        public PathNavigateGround(EntityMob entity, WorldServer worldIn) : base(entity, worldIn) { }

        /// <summary>
        /// Сущность может плавать
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CanSwim(bool canSwim) => nodeProcessor.CanSwim = canSwim;
        /// <summary>
        /// Следует избегать воды
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAvoidWater(bool avoidWater) => nodeProcessor.AvoidsWater = avoidWater;
        /// <summary>
        /// Следует избегать лавы и огня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAvoidLavaOrFire(bool avoidLavaOrFire) => nodeProcessor.AvoidsLavaOrFire = avoidLavaOrFire;
        /// <summary>
        /// Ходить через дверь
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetThroughDdoor(bool throughDdoor) => nodeProcessor.ThroughDdoor = throughDdoor;

        /// <summary>
        /// Получить объект PathFinder
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override PathFinder _GetPathFinder()
        {
            nodeProcessor = new NodeProcessorWalk();
            return new PathFinder(_world, nodeProcessor);
        }

        /// <summary>
        /// Если на земле или в плавании и может плавать
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _CanNavigate()
            => _entity.OnGround || nodeProcessor.CanSwim && _IsInLiquid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Vector3 _GetEntityPosition()
            => new Vector3(_entity.PosX, _GetPositionHeight(), _entity.PosZ);

        /// <summary>
        /// Определить высоту позиции сущности
        /// </summary>
        private int _GetPositionHeight()
        {
            if (_entity.PresenceBlocks.IsInWater() && nodeProcessor.CanSwim)
            {
                int y = (int)_entity.PosY;
                BlockBase block = _world.GetBlockState(new BlockPos(
                    Mth.Floor(_entity.PosX), y, Mth.Floor(_entity.PosZ))).GetBlock();
                int count = 0;

                do
                {
                    if (block.Material.IndexMaterial != (int)EnumMaterial.Water)
                    {
                        return y;
                    }

                    ++y;
                    block = _world.GetBlockState(new BlockPos(
                        Mth.Floor(_entity.PosX), y, Mth.Floor(_entity.PosZ))).GetBlock();
                    ++count;
                }
                while (count <= 16);

                return (int)_entity.PosY;
            }
            else
            {
                return (int)(_entity.PosY + .5f);
            }
        }

        /// <summary>
        /// Обрезает данные пути от конца до первого блока, покрытого солнцем
        /// </summary>
        protected override void _RemoveSunnyPath()
        {
            base._RemoveSunnyPath();
            if (restrictSun)
            {
                if (_world.IsAgainstSky(new BlockPos(Mth.Floor(_entity.PosX),
                    (int)(_entity.PosY + .5f), Mth.Floor(_entity.PosZ))))
                {
                    return;
                }

                for (int i = 0; i < CurrentPath.GetCurrentPathLength(); ++i)
                {
                    PathPoint var2 = CurrentPath.GetPathPointFromIndex(i);

                    if (_world.IsAgainstSky(new BlockPos(var2.CoordX, var2.CoordY, var2.CoordZ)))
                    {
                        CurrentPath.SetCurrentPathLength(i - 1);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает true, если объект указанного размера может безопасно пройти по прямой линии между двумя точками
        /// </summary>
        protected override bool _IsDirectPathBetweenPoints(Vector3 pos1, Vector3 pos2, 
            int sizeX, int sizeY, int sizeZ)
        {
            int x = Mth.Floor(pos1.X);
            int z = Mth.Floor(pos1.Z);
            float vx = pos2.X - pos1.X;
            float vz = pos2.Z - pos1.Z;
            float sq = vx * vx + vz * vz;

            if (sq < .000001f) return false;

            float k = 1f / Mth.Sqrt(sq);
            vx *= k;
            vz *= k;
            sizeX += 2;
            sizeZ += 2;

            if (!_CheckMove(x, (int)pos1.Y, z, sizeX, sizeY, sizeZ, pos1, vx, vz))
            {
                return false;
            }

            sizeX -= 2;
            sizeZ -= 2;
            float kx = 1f / Mth.Abs(vx);
            float kz = 1f / Mth.Abs(vz);
            float vx2 = x - pos1.X;
            float vz2 = z - pos1.Z;

            if (vx >= 0f) ++vx2;
            if (vz >= 0f) ++vz2;

            vx2 /= vx;
            vz2 /= vz;
            int nx = vx < 0 ? -1 : 1;
            int nz = vz < 0 ? -1 : 1;
            int x2 = Mth.Floor(pos2.X);
            int z2 = Mth.Floor(pos2.Z);
            int v2x = x2 - x;
            int v2z = z2 - z;

            do
            {
                if (v2x * nx <= 0 && v2z * nz <= 0) return true;

                if (vx2 < vz2)
                {
                    vx2 += kx;
                    x += nx;
                    v2x = x2 - x;
                }
                else
                {
                    vz2 += kz;
                    z += nz;
                    v2z = z2 - z;
                }
            }
            while (_CheckMove(x, (int)pos1.Y, z, sizeX, sizeY, sizeZ, pos1, vx, vz));

            return false;
        }

        /// <summary>
        /// Можем ли мы пройти в этой части и не упасть
        /// </summary>
        private bool _CheckMove(int posX, int posY, int posZ, int sizeX, int sizeY, int sizeZ, 
            Vector3 pos, float vecX, float vecZ)
        {
            int x0 = posX - sizeX / 2;
            int z0 = posZ - sizeZ / 2;
            float x2, z2;
            BlockPos blockPos;
            BlockState blockState;

            BlockPos[] blocks = BlockPos.GetAllInBox(new Vector3i(x0, posY, z0), 
                new Vector3i(x0 + sizeX - 1, posY + sizeY - 1, z0 + sizeZ - 1));
            for (int i = 0; i < blocks.Length; i++)
            {
                blockPos = blocks[i];
                x2 = blockPos.X + .5f - pos.X;
                z2 = blockPos.Z + .5f - pos.Z;

                if (x2 * vecX + z2 * vecZ >= 0f)
                {
                    blockState = _world.GetBlockState(blockPos);
                    if (blockState.GetBlock().Passable)
                    {
                        return false;
                    }
                }
            }

            // Определить что под ногами
            for (int x = x0; x < x0 + sizeX; ++x)
            {
                for (int z = z0; z < z0 + sizeZ; ++z)
                {
                    x2 = x + .5f - pos.X;
                    z2 = z + .5f - pos.Z;
                    if (x2 * vecX + z2 * vecZ >= 0.0f)
                    {
                        EnumMaterial material = (EnumMaterial)_world.GetBlockState(
                            new BlockPos(x, posY - 1, z)).GetBlock().Material.IndexMaterial;

                        if (material == EnumMaterial.Air
                            || (material == EnumMaterial.Water && !_entity.PresenceBlocks.IsInWater())
                            || material == EnumMaterial.Lava || material == EnumMaterial.Oil)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
