using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Util;
using Vge.World;

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
        /// Будет ли эта сущность проходить сквозь блоки
        /// </summary>
        public bool NoClip { get; protected set; } = false;

        /// <summary>
        /// Коэффициент рикошета, 0 нет отскока, 1 максимальный
        /// </summary>
        protected readonly float _rebound;
        /// <summary>
        /// Имеется ли рикошет
        /// </summary>
        protected readonly bool _isRebound;
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
        /// Спит ли физика
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPhysicSleep() => _indexSleep == 0;
        /// <summary>
        /// Почти спит ли физика, т.е. следующим тактом должна заснуть, если никто не пробудит
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPhysicAlmostSleep() => _indexSleep == 1;
        /// <summary>
        /// Пробудить физику на 4 такта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwakenPhysics() => _indexSleep = 4; // на 4 такта

        public virtual string ToDebugString() => "";

        /// <summary>
        /// Обновить данные в такте игры
        /// </summary>
        public virtual void LivingUpdate() { }

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
        /// Проверка перемещения со столкновением для медленный сущностей с hitbox
        /// </summary>
        protected void _CheckMoveColliding()
        {
            if (!NoClip)
            {
                AxisAlignedBB boundingBox = Entity.GetBoundingBox();

                // Отрегулировать движение для подкрадывания
                _AdjustMovementForSneaking(boundingBox);

                float x = MotionX;
                float y = MotionY;
                float z = MotionZ;

                AxisAlignedBB aabbEntity = boundingBox.Clone();
                List<AxisAlignedBB> aabbs = Collision.GetStaticBoundingBoxes(boundingBox.AddCoordBias(x, y, z));

                // Находим смещение по Y
                foreach (AxisAlignedBB axis in aabbs) y = axis.CalculateYOffset(aabbEntity, y);
                // Рикошет от препятствия
                if (_isRebound && MotionY != y && MotionY < PhysicsGround.GravityRebound)
                {
                    float y0 = -(MotionY + PhysicsGround.GravityRebound) * _rebound;
                    if (y0 > y) y = y0;
                }
                aabbEntity = aabbEntity.Offset(0, y, 0);

                // Фиксация возможен ли авто прыжок
                _AutoNotJump(y);

                // Находим смещение по X
                foreach (AxisAlignedBB axis in aabbs) x = axis.CalculateXOffset(aabbEntity, x);
                // Рикошет от препятствия
                if (_isRebound && MotionX != x) x = -MotionX * _rebound;

                aabbEntity = aabbEntity.Offset(x, 0, 0);

                // Находим смещение по Z
                foreach (AxisAlignedBB axis in aabbs) z = axis.CalculateZOffset(aabbEntity, z);
                // Рикошет от препятствия
                if (_isRebound && MotionZ != z) z = -MotionZ * _rebound;

                // Авто прыжок
                _AutoJump(boundingBox, ref x, ref y, ref z);

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
            List<EntityBase> entities = Collision.GetEntityBoundingBoxesFromSector(boundingBox.Expand(4), Entity.Id);

            // Если нет сущностей, то не зачем дальше обрабатывать
            if (entities.Count == 0) return;

            AxisAlignedBB aabbEntity = boundingBox.Clone();
            // Коробка для проверки
            AxisAlignedBB aabbCheck;
            float f;
            float x0 = x;
            float y0 = y;
            float z0 = z;

            // == ВЕРТИКАЛЬ

            if (y > .006f)//PhysicsGround.GravityRebound) // Тут мы тогда когда 
            {
                // Проверяем что сверху когда прыгаем
                // +-----+
                // | *** | Эта рамка будет
                // +-----+
                // |  я  |
                // +-----+
                aabbCheck = boundingBox.Up(y);

                // Находим смещение по Y
                foreach (EntityBase entity in entities)
                {
                    if (aabbCheck.IntersectsWith(entity.GetBoundingBox()))
                    {
                        f = y;
                        y = entity.GetBoundingBox().CalculateYOffset(aabbEntity, f);
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
                foreach (EntityBase entity in entities)
                {
                    if (aabbCheck.IntersectsWith(entity.GetBoundingBox()))
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
                    foreach (EntityBase entity in entities)
                    {
                        if (aabbCheck.IntersectsWith(entity.GetBoundingBox()))
                        {
                            y = entity.GetBoundingBox().CalculateYOffset(aabbEntity, y);
                        }
                    }
                    // Защита от мелких смещений, чтоб если сущности на уровне, можно было ходить по ним
                    if (y0 != y) y += .02f;

                    // Рикошет от сущности через импульс
                    if (_isRebound && y0 != y) ImpulseY -= (y0 + PhysicsGround.GravityRebound) * _rebound;
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
                    foreach (EntityBase entity in entities)
                    {
                        if (aabbCheck.IntersectsWith(entity.GetBoundingBox()))
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
            foreach (EntityBase entity in entities)
            {
                if (aabbCheck.IntersectsWith(entity.GetBoundingBox()))
                {
                    f = x;
                    x = entity.GetBoundingBox().CalculateXOffset(aabbEntity, f);
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
            foreach (EntityBase entity in entities)
            {
                if (aabbCheck.IntersectsWith(entity.GetBoundingBox()))
                {
                    f = z;
                    z = entity.GetBoundingBox().CalculateZOffset(aabbEntity, f);
                    if (entity.CanBePushed() && f != z)
                    {
                        Entity.SetEntityPhysicsImpulse(entity, 0, 0, z0 * entity.GetWeightImpulse(Entity));
                    }
                }
            }
            // Рикошет от сущности через импульс
            if (_isRebound && z0 != z) ImpulseZ -= z0 * _rebound;

            // == ВЫТАЛКИВАНИЕ
            // Для игрока тоже остаётся, чтоб игроки сами в себе не находились
            aabbCheck = boundingBox.Offset(x, y, z);
            foreach (EntityBase entity in entities)
            {
                if (aabbCheck.IntersectsWith(entity.GetBoundingBox()))
                {
                    if (entity.CanBePushed())
                    {
                        entity.ApplyEntityCollision(Entity);
                    }
                }
            }
        }
    }
}
