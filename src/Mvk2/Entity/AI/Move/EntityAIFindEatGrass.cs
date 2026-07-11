using Mvk2.World.Block;
using System;
using Vge.Entity;
using Vge.Entity.AI;
using Vge.Network.Packets.Server;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using WinGL.Util;

namespace Mvk2.Entity.AI
{
    /// <summary>
    /// Задача найти еду, цветок или траву с вероятностью 30% или 70% дёрн, подойти к нему и съесть
    /// </summary>
    public class EntityAIFindEatGrass : EntityAIBaseMove
    {
        /// <summary>
        /// Частота вероятности сработки задачи
        /// </summary>
        private readonly float _probability;


        /// <summary>
        /// Действие перемещения
        /// </summary>
        private bool _actionMove;
        /// <summary>
        /// Действие кушать
        /// </summary>
        private bool _actionEat;
        /// <summary>
        /// Действие конечной анимации
        /// </summary>
        private bool _actionEnd;
        /// <summary>
        /// Время в тиках на поедание блока
        /// </summary>
        private int _timeEat;
        /// <summary>
        /// Время в тиках на конечной анимации
        /// </summary>
        private int _timeEnd;
        /// <summary>
        /// Ищем траву
        /// </summary>
        private bool _isSapling = false;

        /// <summary>
        /// Задача найти еду, цветок или траву с вероятностью 30% или 70% дёрн, подойти к нему и съесть
        /// </summary>
        public EntityAIFindEatGrass(EntityMob entity, float probability = .1f, float speed = 1f) 
            : base(entity, speed)
        {
            _probability = probability;
        }

        /// <summary>
        /// Возвращает значение, указывающее, следует ли начать выполнение
        /// </summary>
        public override bool ShouldExecute()
        {
            if (_entity.EntityAge < 200 && Rnd.NextFloat() < _probability)
            {
                _xPosition = _entity.PosX;
                _yPosition = _entity.PosY - 1;
                _zPosition = _entity.PosZ;

                // Проверка наличия растения под ногами
                if (_Check(_xPosition, _yPosition, _zPosition))
                {
                    _EatBegin();
                    return true;
                }

                // Ищем блок увеличивая радиус
                if (_FindBlock(_entity.GetWorldServer(), 8)) return true;
                if (_FindBlock(_entity.GetWorldServer(), 12)) return true;
                if (_FindBlock(_entity.GetWorldServer(), 16)) return true;
                if (_FindBlock(_entity.GetWorldServer(), 24)) return true;
                if (_FindBlock(_entity.GetWorldServer(), 32)) return true;
            }
            return false;
        }

        /// <summary>
        /// Поиск блока, через рандом по дистанции
        /// </summary>
        private bool _FindBlock(WorldServer worldServer, int bxz)
        {
            float x0 = _xPosition + Rnd.Next(bxz) - Rnd.Next(bxz);
            float z0 = _zPosition + Rnd.Next(bxz) - Rnd.Next(bxz);

            BlockPos blockPos = new BlockPos(x0, _yPosition, z0);
            ChunkBase chunk = worldServer.GetChunk(blockPos.GetPositionChunk());
            BlockBase block;
            int y = 0;
            if (chunk != null)
            {
                int x = blockPos.X & 15;
                int z = blockPos.Z & 15;
                for (int y0 = -6; y0 < 8; y0++)
                {
                    y = blockPos.Y + y0;
                    if (y > 0 && y < chunk.Settings.NumberBlocks)
                    {
                        block = chunk.GetBlockStateNotCheck(x, y, z).GetBlock();
                        if (block.IndexBlock == BlocksRegMvk.Turf.IndexBlock
                            || block.IndexBlock == BlocksRegMvk.TurfLoam.IndexBlock)
                        {
                            // Ура
                            _xPosition = x0;
                            _yPosition = y + 1;
                            _zPosition = z0;
                            //Console.WriteLine("E!!! " + _xPosition + " " + _yPosition + " " + _zPosition);

                            //worldServer.SpawnParticle(EntitiesFXReg.CubeId, 1,
                            //    new Vector3(_xPosition, _yPosition, _zPosition),
                            //    new Vector3(0), 0, 0x4F06C);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Проверка нахождении нужного блока
        /// </summary>
        protected virtual bool _Check(float x, float y, float z)
        {
            int id = _entity.GetWorldServer().GetBlockState(new BlockPos(x, y, z)).Id;
            return id == BlocksRegMvk.Turf.IndexBlock || id == BlocksRegMvk.TurfLoam.IndexBlock;
        }

        /// <summary>
        /// Начинаем кушать
        /// </summary>
        private void _EatBegin()
        {
            _actionMove = false;
            _actionEat = true;
            _timeEat = 25;
            //_entity.MoveHelper.SetForward(.1f);
            //_entity.LookHelper.SetLookPitch(-Glm.Pi90, Glm.Pi10);
            _entity.GetWorldServer().Tracker.SendToAllTrackingEntity(_entity.Id,
                       new PacketS0BAnimation(_entity.Id, "Peck"));
        }

        /// <summary>
        /// Обновляет задачу
        /// </summary>
        public override void UpdateTask()
        {
            if (_actionMove)
            {
                if (_entity.Navigator.NoPath()) _EatBegin();
            }
            else if (_actionEat)
            {
                if (--_timeEat <= 0)
                {
                    _actionEat = false;
                    if (_Action(new BlockPos(_entity.PosX, _entity.PosY, _entity.PosZ)))
                    {
                        _actionEnd = true;
                        _timeEnd = 70;
                        _entity.GetWorldServer().Tracker.SendToAllTrackingEntity(_entity.Id,
                            new PacketS0BAnimation(_entity.Id, "Drip"));
                    }
                }
            }
            else if (_actionEnd)
            {
                // Процесс, чтоб отработала анимация
                if (--_timeEnd <= 0)
                {
                    _actionEnd = false;
                }
            }
        }

        /// <summary>
        /// Действие когда дошли до блока
        /// </summary>
        private bool _Action(BlockPos blockPos)
        {
            if (_entity.GetWorldServer().GetBlockState(blockPos).GetBlock().Material.IndexMaterial
                == (int)EnumMaterial.Plant)
            {
                // Растение
                _entity.GetWorldServer().SetBlockToAir(blockPos, 31);
                _Eat();
                return false;

            }
            blockPos = blockPos.OffsetDown();
            int id = _entity.GetWorldServer().GetBlockState(blockPos).Id;

            if (id == BlocksRegMvk.Turf.IndexBlock)
            {
                _entity.GetWorldServer().SetBlockState(blockPos, new BlockState(BlocksRegMvk.Humus.IndexBlock), 31);
                _Eat();
            }
            else if (id == BlocksRegMvk.TurfLoam.IndexBlock)
            {
                _entity.GetWorldServer().SetBlockState(blockPos, new BlockState(BlocksRegMvk.Loam.IndexBlock), 31);
                _Eat();
            }

            return true;
        }

        /// <summary>
        /// Кушаем
        /// </summary>
        private void _Eat()
        {
            //if (entity is EntityChicken entityChicken && entityChicken.countEat < 10)
            //{
            //    entityChicken.countEat++;
            //}
        }

        /// <summary>
        /// Возвращает значение, указывающее, должна ли незавершенная тикущая задача продолжать выполнение
        /// </summary>
        public override bool ContinueExecuting() => _actionMove || _actionEat || _actionEnd;

        /// <summary>
        /// Выполните разовую задачу или начните выполнять непрерывную задачу
        /// </summary>
        public override void StartExecuting()
        {
            if (!_actionEat && _entity.Navigator.TryMoveToXYZ(_xPosition, _yPosition, _zPosition, _speed, false))
            {
                _actionMove = true;
            }
        }

        /// <summary>
        /// Сбрасывает задачу
        /// </summary>
        public override void ResetTask()
        {
            _actionMove = false;
            _actionEat = false;
            _actionEnd = false;
        }

    }
}
