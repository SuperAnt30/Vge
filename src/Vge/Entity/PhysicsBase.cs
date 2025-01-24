using System.Collections.Generic;
using Vge.Util;
using Vge.World;

namespace Vge.Entity
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
        /// Имеется ли сила для движения
        /// </summary>
        protected readonly bool _isForceForMovement;
        /// <summary>
        /// Коэффициент отскока, 0 нет отскока, 1 максимальный
        /// </summary>
        protected readonly float _rebound;
        /// <summary>
        /// Имеется ли отскок
        /// </summary>
        protected readonly bool _isRebound;

        /// <summary>
        /// Физика для сущности которая имеет силу для перемещения
        /// </summary>
        /// <param name="inputMovement">Используется ли у сущности силы действия перемещения</param>
        public PhysicsBase(CollisionBase collision, EntityBase entity)
        {
            Collision = collision;
            Entity = entity;
            Movement = new MovementInput();
            _isForceForMovement = true;
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
           // if (_rebound > 1) _rebound = 1;
            _isRebound = _rebound != 0;
        }

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
        /// Проверка перемещения со столкновением
        /// </summary>
        protected void _CheckMoveCollidingEntity()
        {
            if (!NoClip)
            {
                AxisAlignedBB boundingBox = Entity.GetBoundingBox();

                _AdjustMovementForSneaking(boundingBox);

                float x = MotionX;
                float y = MotionY;
                float z = MotionZ;

                AxisAlignedBB aabbEntity = boundingBox.Clone();
                List<AxisAlignedBB> aabbs = Collision.GetCollidingBoundingBoxes(boundingBox.AddCoordBias(x, y, z), Entity.Id);

                // Находим смещение по Y
                foreach (AxisAlignedBB axis in aabbs) y = axis.CalculateYOffset(aabbEntity, y);
                // Отскок от препятствия
                if (_isRebound && MotionY != y && MotionY < PhysicsGround.GravityRebound)
                {
                    float y0 = -(MotionY + PhysicsGround.GravityRebound) * _rebound;
                    if (y0 > y) y = y0;
                }
                aabbEntity = aabbEntity.Offset(0, y, 0);

                _AutoNotJump(y);

                // Находим смещение по X
                foreach (AxisAlignedBB axis in aabbs) x = axis.CalculateXOffset(aabbEntity, x);
                // Отскок от препятствия
                if (_isRebound && MotionX != x) x = -MotionX * _rebound;
                aabbEntity = aabbEntity.Offset(x, 0, 0);

                // Находим смещение по Z
                foreach (AxisAlignedBB axis in aabbs) z = axis.CalculateZOffset(aabbEntity, z);
                // Отскок от препятствия
                if (_isRebound && MotionZ != z) z = -MotionZ * _rebound;

                _AutoJump(boundingBox, ref x, ref y, ref z);

                Entity.OnGround = MotionY != y && MotionY < 0.0f;

                MotionX = x;
                MotionY = y;
                MotionZ = z;
            }
        }
    }
}
