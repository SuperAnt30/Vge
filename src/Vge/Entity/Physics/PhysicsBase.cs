using System.Runtime.CompilerServices;
using Vge.Entity.Sizes;
using Vge.Util;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity.Physics
{
    /// <summary>
    /// Базовый объект физики для сущности
    /// </summary>
    public class PhysicsBase
    {
        /// <summary>
        /// Обект перемещений
        /// </summary>
        public readonly MovementInput Movement;
        /// <summary>
        /// Сущность к которой прекреплена физика
        /// </summary>
        public readonly EntityBase Entity;
        /// <summary>
        /// Объект проверки коллизии
        /// </summary>
        public readonly CollisionBase Collision;

        /// <summary>
        /// Перемещение за текущий такт по координте X
        /// </summary>
        public float MotionX;
        /// <summary>
        /// Перемещение за текущий такт по координте Y
        /// </summary>
        public float MotionY;
        /// <summary>
        /// Перемещение за текущий такт по координте Z
        /// </summary>
        public float MotionZ;

        /// <summary>
        /// Импульс по координте X
        /// </summary>
        public float ImpulseX;
        /// <summary>
        /// Импульс по координте Y
        /// </summary>
        public float ImpulseY;
        /// <summary>
        /// Импульс по координте Z
        /// </summary>
        public float ImpulseZ;

        /// <summary>
        /// Перемещение по горизонту
        /// </summary>
        public float MotionHorizon { get; protected set; }
        /// <summary>
        /// Перемещение по вертикали
        /// </summary>
        public float MotionVertical { get; protected set; }

        /// <summary>
        /// Было ли перемещение
        /// </summary>
        public bool IsMotionChange { get; protected set; } = false;
        /// <summary>
        /// Было ли смена позы или ускорение
        /// </summary>
        public bool IsPoseChange { get; protected set; } = false;
        /// <summary>
        /// Сколько тактов сущность зажата в блоке
        /// </summary>
        public ushort CaughtInBlock { get; private set; } = 0;
        /// <summary>
        /// Сколько тактов сущность зажата в другой сущности
        /// </summary>
        public ushort CaughtInEntity { get; private set; } = 0;

        /// <summary>
        /// Коэффициент рикошета, 0 нет отскока, 1 максимальный
        /// </summary>
        private float _rebound;
        /// <summary>
        /// Имеется ли рикошет
        /// </summary>
        private bool _isRebound;
        /// <summary>
        /// Индекс для сна, 0 спит, больше одного, количество тактов до сна
        /// </summary>
        protected byte _indexSleep;

        /// <summary>
        /// Физика для сущности которая имеет силу для перемещения
        /// </summary>
        /// <param name="inputMovement">Используется ли у сущности силы действия перемещения</param>
        public PhysicsBase(CollisionBase collision, EntityBase entity)
        {
            Collision = collision;
            Entity = entity;
            Movement = new MovementInput();
            AwakenPhysics();
        }

        /// <summary>
        /// Физика для предмета которые не имеет силы для перемещения но может имет отскок от предметов
        /// </summary>
        /// <param name="rebound">Коэффициент отскока, 0 нет отскока, 1 максимальный</param>
        public PhysicsBase(CollisionBase collision, EntityBase entity, float rebound)
        {
            Collision = collision;
            Entity = entity;
            _rebound = rebound;
            _isRebound = _rebound != 0;
            AwakenPhysics();
        }

        /// <summary>
        /// Задать коэффициент рикошета
        /// </summary>
        /// <param name="rebound">Коэффициент отскока, 0 нет отскока, 1 максимальный</param>
        public void SetRebound(float rebound)
        {
            _rebound = rebound;
            _isRebound = _rebound != 0;
        }

        /// <summary>
        /// Спит ли физика
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPhysicSleep() => _indexSleep == 0;
        /// <summary>
        /// Пробудить физику на 4 такта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwakenPhysics() => _indexSleep = 4;

        /// <summary>
        /// Действие сущность не зажата в другой сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _NotClampedEnity()
        {
            if (CaughtInEntity > 0)
            {
                if (CaughtInEntity == 1) CaughtInEntity = 0;
                else CaughtInEntity -= 2;
                AwakenPhysics();
            }
        }

        /// <summary>
        /// Действие сущность не зажата в блоке
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _NotClampedBlock()
        {
            if (CaughtInBlock > 0)
            {
                if (CaughtInBlock == 1) CaughtInBlock = 0;
                else CaughtInBlock -= 2;
                AwakenPhysics();
            }
        }

        public virtual string ToDebugString() => "";

        /// <summary>
        /// Обновить данные в такте игры
        /// </summary>
        public virtual void LivingUpdate() { }

        /// <summary>
        /// Сбросить изменение состоянии позы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetPose()
        {
            if (IsPoseChange)
            {
                IsPoseChange = false;
            }
        }

        /// <summary>
        /// Отрегулировать движение для подкрадывания
        /// </summary>
        protected virtual void _AdjustMovementForSneaking(AxisAlignedBB boundingBox) { }

        /// <summary>
        /// Фиксация возможен ли авто прыжок
        /// </summary>
        protected virtual void _AutoNotJump(float y) { }
        /// <summary>
        /// Авто прыжок
        /// </summary>
        protected virtual void _AutoJump(AxisAlignedBB boundingBox, ref float x, ref float y, ref float z) { }

        /// <summary>
        /// Лимит по максимальному импульсу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void _ImpulseLimit()
        {
            if (ImpulseX != 0)
            {
                if (ImpulseX > Ce.MaxImpulse) MotionX += Ce.MaxImpulse;
                else if (ImpulseX < -Ce.MaxImpulse) MotionX -= Ce.MaxImpulse;
                else MotionX += ImpulseX;
                ImpulseX = 0;
            }
            if (ImpulseY != 0)
            {
                if (ImpulseY > Ce.MaxImpulse) MotionY += Ce.MaxImpulse;
                else if (ImpulseY < -Ce.MaxImpulse) MotionY -= Ce.MaxImpulse;
                else MotionY += ImpulseY;
                ImpulseY = 0;
            }
            if (ImpulseZ != 0)
            {
                if (ImpulseZ > Ce.MaxImpulse) MotionZ += Ce.MaxImpulse;
                else if (ImpulseZ < -Ce.MaxImpulse) MotionZ -= Ce.MaxImpulse;
                else MotionZ += ImpulseZ;
                ImpulseZ = 0;
            }
        }

        /// <summary>
        /// Если мелочь убираем
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void _ResetMinimumMotion()
        {
            if (Mth.Abs(MotionX) < .005f) MotionX = 0;
            if (Mth.Abs(MotionY) < .005f) MotionY = 0;
            if (Mth.Abs(MotionZ) < .005f) MotionZ = 0;
        }

        /// <summary>
        /// Проверка перемещения со столкновением для медленный сущностей с hitbox
        /// </summary>
        protected void _CheckMoveColliding(ISizeEntityBox size)
        {
            if (!Entity.NoClip)
            {
                AxisAlignedBB boundingBox = size.GetBoundingBox();

                // Отрегулировать движение для подкрадывания
                _AdjustMovementForSneaking(boundingBox);

                float x = MotionX;
                float y = MotionY;
                float z = MotionZ;

                AxisAlignedBB aabbEntity = boundingBox.Clone();
                Collision.StaticBoundingBoxes(boundingBox.AddCoordBias(x, y, z));
                ListFast<AxisAlignedBB> aabbs = Collision.ListBlock;
                int count = aabbs.Count;
                if (count > 0)
                {
                    // Находим смещение по Y
                    for (int i = 0; i < count; i++) y = aabbs[i].CalculateYOffset(aabbEntity, y);
                    // Рикошет от препятствия
                    if (_isRebound && MotionY != y && MotionY < Cp.GravityRebound)
                    {
                        float y0 = -(MotionY + Cp.GravityRebound) * _rebound;
                        if (y0 > y) y = y0;
                    }
                    /*if (MotionY != y)*/ aabbEntity = aabbEntity.Offset(0, y, 0);

                    // Фиксация возможен ли авто прыжок
                    _AutoNotJump(y);

                    // Находим смещение по X
                    for (int i = 0; i < count; i++) x = aabbs[i].CalculateXOffset(aabbEntity, x);
                    // Рикошет от препятствия
                    if (_isRebound && MotionX != x) x = -MotionX * _rebound;
                    /*if (MotionX != x)*/ aabbEntity = aabbEntity.Offset(x, 0, 0);

                    // Находим смещение по Z
                    for (int i = 0; i < count; i++) z = aabbs[i].CalculateZOffset(aabbEntity, z);
                    // Рикошет от препятствия
                    if (_isRebound && MotionZ != z) z = -MotionZ * _rebound;

                    // Проверка находится ли сущность в блоке
                    bool caughtInBlock = false;
                    aabbEntity = size.GetBoundingBoxOffset(x, y, z);
                    for (int i = 0; i < count; i++)
                    {
                        if (aabbs[i].IntersectsWith(aabbEntity))
                        {
                            caughtInBlock = true;
                            break;
                        }
                    }
                    // Пометки зажатости
                    if (!caughtInBlock)
                    {
                        _NotClampedBlock();
                    }
                    else
                    {
                        if (CaughtInBlock < 65535)
                        {
                            CaughtInBlock++;
                            AwakenPhysics();
                        }
                    }

                    Collision.ListBlock.ClearFull();

                    // Авто прыжок
                    _AutoJump(boundingBox, ref x, ref y, ref z);
                }
                else
                {
                    _NotClampedBlock();
                }

                // Проверка перемещения со столкновением сущностей
                _CheckMoveCollidingEntity(boundingBox, ref x, ref y, ref z);

                Entity.OnGround = MotionY != y && MotionY < 0.0f;

                MotionX = x;
                MotionY = y;
                MotionZ = z;
            }
        }

        /// <summary>
        /// Проверка перемещения со столкновением сущностей
        /// </summary>
        protected void _CheckMoveCollidingEntity(AxisAlignedBB boundingBox, ref float x, ref float y, ref float z)
        {
            // Собираем все близлижащий сущностей для дальнейше проверки
            Collision.EntityBoundingBoxesFromSector(boundingBox.AddCoordBias(x, y, z).Expand(.2f), Entity.Id);

            // Если нет сущностей, то не зачем дальше обрабатывать
            if (Collision.ListEntity.Count == 0)
            {
                _NotClampedEnity();
                return;
            }

            ListFast<EntityBase> entities = Collision.ListEntity;
            int count = entities.Count;
            EntityBase entity;

            AxisAlignedBB aabbEntity = boundingBox.Clone();
            // Коробка для проверки
            AxisAlignedBB aabbCheck;
            float f;
            float x0 = x;
            float y0 = y;
            float z0 = z;

            // == ВЕРТИКАЛЬ

            if (y > .006f) // Тут мы тогда когда происходит взлёт
            {
                // Проверяем что сверху когда прыгаем
                // +-----+
                // | *** | Эта рамка будет
                // +-----+
                // |  я  |
                // +-----+
                aabbCheck = boundingBox.Up(y);

                // Находим смещение по Y
                for (int i = 0; i < count; i++)
                {
                    entity = entities[i];
                    if (entity.Size.IntersectsWith(aabbCheck))
                    {
                        f = y;
                        y = entity.Size.CalculateYOffset(aabbEntity, f);
                        if (entity.CanBePushed() && f != y)
                        {
                            Entity.SetEntityPhysicsImpulse(entity, 0, y0 * entity.GetWeightImpulse(Entity), 0);
                        }
                    }
                }

                aabbEntity = aabbEntity.Offset(0, y, 0);
            }
            else
            {
                // Пробуждаем кто сверху
                // +-----+
                // | *** | Эта рамка будет
                // +-----+
                // |  я  |
                // +-----+
                aabbCheck = boundingBox.Up(.2f);
                for (int i = 0; i < count; i++)
                {
                    entity = entities[i];
                    if (entity.Size.IntersectsWith(aabbCheck))
                    {
                        Entity.AwakenPhysicSleep(entity);
                    }
                }
                if (y < -.005f)
                {
                    // Проверяем что снизу
                    // +-----+
                    // |  я  |
                    // +-----+
                    // | *** | Это рамка будет
                    // +-----+
                    aabbCheck = boundingBox.Down(y);
                    // Находим смещение по Y
                    for (int i = 0; i < count; i++)
                    {
                        entity = entities[i];
                        if (entity.Size.IntersectsWith(aabbCheck))
                        {
                            y = entity.Size.CalculateYOffset(aabbEntity, y);
                        }
                    }
                    // Защита от мелких смещений, чтоб если сущности на уровне, можно было ходить по ним
                    if ((x != 0 || z != 0) && y0 != y) y += .02f;

                    // Рикошет от сущности через импульс
                    if (_isRebound && y0 != y) ImpulseY -= (y0 + Cp.GravityRebound) * _rebound;
                    aabbEntity = aabbEntity.Offset(0, y, 0);
                }
                else
                {
                    // Если ходим по земле, то надо тащить тех кто сверху
                    // +-----+
                    // | *** | Эта рамка будет
                    // +-----+
                    // |  я  |
                    // +-----+
                    aabbCheck = boundingBox.Up(.2f);
                    for (int i = 0; i < count; i++)
                    {
                        entity = entities[i];
                        if (entity.Size.IntersectsWith(aabbCheck))
                        {
                            // Создаём импульс кто сверху для перемещения
                            float force = entity.GetWeightImpulse(Entity) * .25f;
                            Entity.SetEntityPhysicsImpulse(entity, x0 * force, 0, z0 * force);
                        }
                    }
                }
            }

            // == ГОРИЗОНТ
            aabbCheck = aabbEntity.AddCoordBias(x, y, z);
            // Находим смещение по X
            for (int i = 0; i < count; i++)
            {
                entity = entities[i];
                if (entity.Size.IntersectsWith(aabbCheck))
                {
                    f = x;
                    x = entity.Size.CalculateXOffset(aabbEntity, f);
                    if (entity.CanBePushed() && f != x)
                    {
                        Entity.SetEntityPhysicsImpulse(entity, x0 * entity.GetWeightImpulse(Entity), 0, 0);
                    }
                }
            }
            // Рикошет от сущности через импульс
            if (_isRebound && x0 != x) ImpulseX -= x0 * _rebound;
            aabbEntity = aabbEntity.Offset(x, 0, 0);

            // Находим смещение по Z
            for (int i = 0; i < count; i++)
            {
                entity = entities[i];
                if (entity.Size.IntersectsWith(aabbCheck))
                {
                    f = z;
                    z = entity.Size.CalculateZOffset(aabbEntity, f);
                    if (entity.CanBePushed() && f != z)
                    {
                        Entity.SetEntityPhysicsImpulse(entity, 0, 0, z0 * entity.GetWeightImpulse(Entity));
                    }
                }
            }
            // Рикошет от сущности через импульс
            if (_isRebound && z0 != z) ImpulseZ -= z0 * _rebound;

            // == ВЫТАЛКИВАНИЕ

            // Проверка находится ли сущность в другой сущности
            bool caughtInEntity = false;

            // Для игрока тоже остаётся, чтоб игроки сами в себе не находились
            aabbCheck = boundingBox.Offset(x, y, z);
            for (int i = 0; i < count; i++)
            {
                entity = entities[i];
                if (entity.Size.IntersectsWith(aabbCheck))
                {
                    if (entity.CanBePushed())
                    {
                        entity.ApplyEntityCollision(Entity);
                    }
                    caughtInEntity = true;
                }
            }
            // Пометки зажатости
            if (!caughtInEntity)
            {
                _NotClampedEnity();
            }
            else
            {
                if (CaughtInEntity < 65535)
                {
                    CaughtInEntity++;
                    AwakenPhysics();
                }
            }
            Collision.ListBlock.ClearFull();
        }


        /// <summary>
        /// Проверка перемещения со столкновением для быстрых сущностей точек, балистики
        /// </summary>
        protected void _CheckMoveCollidingPoint()
        {
            if (!Entity.NoClip)
            {
                Vector3 dir = new Vector3(MotionX, MotionY, MotionZ);
                Collision.RayCast(Entity.PosX, Entity.PosY, Entity.PosZ,
                    dir.Normalize(), Glm.Distance(dir), false, Entity.Id);

                if (Collision.MovingObject.IsCollision())
                {
                    Entity.OnImpact(Collision.World, Collision.MovingObject);
                }
            }
        }
    }
}
