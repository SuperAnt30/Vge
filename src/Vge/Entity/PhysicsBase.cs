using System.Collections.Generic;
using Vge.Util;
using Vge.World;
using WinGL.Util;

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
        public readonly MovementInput Movement = new MovementInput();
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
        public float MotionX;// { get; protected set; }
        /// <summary>
        /// Перемещение за текущий такт по координте Y
        /// </summary>
        public float MotionY;// { get; protected set; }
        /// <summary>
        /// Перемещение за текущий такт по координте Z
        /// </summary>
        public float MotionZ;// { get; protected set; }

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

        public PhysicsBase(CollisionBase collision, EntityBase entity)
        {
            Collision = collision;
            Entity = entity;
        }

        public virtual string ToDebugString() => "";

        /// <summary>
        /// Обновить данные в такте игры
        /// </summary>
        public virtual void LivingUpdate() { }

        /// <summary>
        /// Проверка перемещения со столкновением
        /// </summary>
        protected void _CheckMoveCollidingEntity()
        {
            AxisAlignedBB boundingBox = Entity.GetBoundingBox();
           
            float x0 = MotionX;
            float y0 = MotionY;
            float z0 = MotionZ;

            float x = x0;
            float y = y0;
            float z = z0;
            AxisAlignedBB aabbEntity = boundingBox.Clone();
            List<AxisAlignedBB> aabbs = Collision.GetCollidingBoundingBoxes(boundingBox.AddCoordBias(x, y, z), Entity.Id);

            // Находим смещение по Y
            foreach (AxisAlignedBB axis in aabbs) y = axis.CalculateYOffset(aabbEntity, y);
            //TODO:: Отскок от препятствия
            //if (y0 != y) y = (y - y0 + y) * .5f;
            aabbEntity = aabbEntity.Offset(0, y, 0);

            // Не прыгаем (момент взлёта)
            bool isNotJump = Entity.OnGround || MotionY != y && MotionY < 0f;

            // Находим смещение по X
            foreach (AxisAlignedBB axis in aabbs) x = axis.CalculateXOffset(aabbEntity, x);
            //TODO:: Отскок от препятствия
            //if (x0 != x) x = (x - x0 + x) * .5f;
            aabbEntity = aabbEntity.Offset(x, 0, 0);

            // Находим смещение по Z
            foreach (AxisAlignedBB axis in aabbs) z = axis.CalculateZOffset(aabbEntity, z);
            //TODO:: Отскок от препятствия
            //if (z0 != z) z = (z - z0 + z) * .5f;
            aabbEntity = aabbEntity.Offset(0, 0, z);

            // Авто прыжок
            float StepHeight = 0.7f;// 1.2f;
            // Запуск проверки авто прыжка
            if (StepHeight > 0f && isNotJump && (x0 != x || z0 != z))
            {
                // Кэш для откада, если авто прыжок не допустим
                float monCacheX = x;
                float monCacheY = y;
                float monCacheZ = z;

                float stepHeight = StepHeight;
                // Если сидим авто прыжок в двое ниже
                if (Movement.Sneak) stepHeight *= 0.5f;

                y = stepHeight;
                aabbs = Collision.GetCollidingBoundingBoxes(boundingBox.AddCoordBias(x0, y, z0), Entity.Id);
                AxisAlignedBB aabbEntity2 = boundingBox.Clone();
                AxisAlignedBB aabb = aabbEntity2.AddCoordBias(x0, 0, z0);

                // Находим смещение по Y
                float y2 = y;
                foreach (AxisAlignedBB axis in aabbs) y2 = axis.CalculateYOffset(aabb, y2);
                aabbEntity2 = aabbEntity2.Offset(0, y2, 0);

                // Находим смещение по X
                float x2 = x0;
                foreach (AxisAlignedBB axis in aabbs) x2 = axis.CalculateXOffset(aabbEntity2, x2);
                aabbEntity2 = aabbEntity2.Offset(x2, 0, 0);

                // Находим смещение по Z
                float z2 = z0;
                foreach (AxisAlignedBB axis in aabbs) z2 = axis.CalculateZOffset(aabbEntity2, z2);
                aabbEntity2 = aabbEntity2.Offset(0, 0, z2);

                AxisAlignedBB aabbEntity3 = boundingBox.Clone();

                // Находим смещение по Y
                float y3 = y;
                foreach (AxisAlignedBB axis in aabbs) y3 = axis.CalculateYOffset(aabbEntity3, y3);
                aabbEntity3 = aabbEntity3.Offset(0, y3, 0);

                // Находим смещение по X
                float x3 = x0;
                foreach (AxisAlignedBB axis in aabbs) x3 = axis.CalculateXOffset(aabbEntity3, x3);
                aabbEntity3 = aabbEntity3.Offset(x3, 0, 0);

                // Находим смещение по Z
                float z3 = z0;
                foreach (AxisAlignedBB axis in aabbs) z3 = axis.CalculateZOffset(aabbEntity3, z3);
                aabbEntity3 = aabbEntity3.Offset(0, 0, z3);

                if (x2 * x2 + z2 * z2 > x3 * x3 + z3 * z3)
                {
                    x = x2;
                    z = z2;
                    aabbEntity = aabbEntity2;
                }
                else
                {
                    x = x3;
                    z = z3;
                    aabbEntity = aabbEntity3;
                }
                y = -stepHeight;

                // Находим итоговое смещение по Y
                foreach (AxisAlignedBB axis in aabbs) y = axis.CalculateYOffset(aabbEntity, y);

                if (monCacheX * monCacheX + monCacheZ * monCacheZ >= x * x + z * z)
                {
                    // Нет авто прыжка, откатываем значение обратно
                    x = monCacheX;
                    y = monCacheY;
                    z = monCacheZ;
                }
                else
                {
                    // Авто прыжок
                    Entity.PosY += y + stepHeight;
                    y = 0;
                }
            }
            Entity.OnGround = y0 != y && y0 < 0.0f;

            MotionX = x;
            MotionY = y;
            MotionZ = z;
        }
    }
}
