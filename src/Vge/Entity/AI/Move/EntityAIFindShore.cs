using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Vge.Entity.AI
{
    /// <summary>
    /// Задача найти берег
    /// </summary>
    public class EntityAIFindShore : EntityAIBaseMove
    {
        /// <summary>
        /// Задача найти берег
        /// </summary>
        public EntityAIFindShore(EntityMob entity, float speed = 1f) : base(entity, speed) { }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (_entity.PresenceBlocks.IsInLiquid && _entity.EntityAge < 200 && Rnd.NextFloat() < .9f)
            {
                _xPosition = _entity.PosX;
                _yPosition = _entity.PosY - 1;
                _zPosition = _entity.PosZ;

                // Ищем  берег увеличивая радиус
                if (_FindShore(_entity.GetWorldServer(), 8)) return true;
                if (_FindShore(_entity.GetWorldServer(), 12)) return true;
                if (_FindShore(_entity.GetWorldServer(), 16)) return true;
                if (_FindShore(_entity.GetWorldServer(), 24)) return true;
                if (_FindShore(_entity.GetWorldServer(), 32)) return true;
            }
            return false;
        }

        /// <summary>
        /// Поиск берега, через рандом по дистанции
        /// </summary>
        private bool _FindShore(WorldServer worldServer, int bxz)
        {
            float x0 = _xPosition + Rnd.Next(bxz) - Rnd.Next(bxz);
            float z0 = _zPosition + Rnd.Next(bxz) - Rnd.Next(bxz);

            BlockPos blockPos = new BlockPos(x0, _yPosition, z0);
            ChunkBase chunk = worldServer.GetChunk(blockPos.GetPositionChunk());
            MaterialBase material;
            BlockBase block;
            int y = 0;
            if (chunk != null)
            {
                int x = blockPos.X & 15;
                int z = blockPos.Z & 15;

                material = chunk.GetBlockStateNotCheck(x, blockPos.Y, z).GetBlock().Material;
                for (int y0 = 1; y0 < 8; y0++)
                {
                    y = blockPos.Y + y0;
                    if (y >= chunk.Settings.NumberBlocks)
                    {
                        break;
                    }
                    block = chunk.GetBlockStateNotCheck(x, y, z).GetBlock();
                    if (material.MoveMob && block.IsAir)
                    {
                        // Ура
                        _xPosition = x0;
                        _yPosition = y;
                        _zPosition = z0;
                        //Console.WriteLine("E!!! " + _xPosition + " " + _yPosition + " " + _zPosition);

                        //worldServer.SpawnParticle(EntitiesFXReg.CubeId, 1,
                        //    new Vector3(_xPosition, _yPosition, _zPosition),
                        //    new Vector3(0), 0, 0x4F06C);
                        return true;
                    }
                    else
                    {
                        material = block.Material;
                    }
                }
            }
            return false;
        }
    }
}
