using Vge.Util;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity.Physics
{
    /// <summary>
    /// Физика для полёта
    /// </summary>
    public class PhysicsFly : PhysicsBase
    {
        /// <summary>
        /// Живая сущность, игрок или моб
        /// </summary>
        private readonly EntityLiving _entityLiving;

        /// <summary>
        /// Физика без гравитации, для полёта 
        /// </summary>
        /// <param name="inputMovement">Используется ли у сущности силы действия перемещения</param>
        public PhysicsFly(CollisionBase collision, EntityLiving entity) 
            : base(collision, entity)
        {
            _entityLiving = entity;
            NoClip = true;
        }
        

        /// <summary>
        /// Обновить данные в такте игры
        /// </summary>
        public override void LivingUpdate()
        {
            // Если нет перемещения по тактам, запускаем трение воздуха
            MotionX *= .98f;
            MotionZ *= .98f;
            // Если мелочь убираем
            if (Mth.Abs(MotionX) < .005f) MotionX = 0;
            if (Mth.Abs(MotionY) < .005f) MotionY = 0;
            if (Mth.Abs(MotionZ) < .005f) MotionZ = 0;

            float speed = .1f;

            Vector2 motion = Sundry.MotionAngle(
               Movement.GetMoveStrafe(), Movement.GetMoveForward(),
               //.5951f 
               .39673f
               * speed * (Movement.Sprinting ? 5f : 1f),
               _entityLiving.RotationYaw);

            // Временно меняем перемещение если это надо
            MotionX += motion.X;
            MotionY += Movement.GetMoveVertical() * speed * 1.0f;
            //if (Movement.Jump)
            //{
            //    MotionY += .84f;
            //}
            
            // * 1.5f;
            MotionZ += motion.Y;

            // Проверка кализии
            IsMotionChange = MotionX != 0 || MotionY != 0 || MotionZ != 0;
            if (IsMotionChange)
            {
                _CheckMoveColliding();

                // Фиксируем перемещение
                IsMotionChange = MotionX != 0 || MotionY != 0 || MotionZ != 0;
            }

            if (IsMotionChange)
            {
                Entity.PosX += MotionX;
                Entity.PosY += MotionY;
                Entity.PosZ += MotionZ;
                MotionHorizon = Glm.Distance(new Vector3(MotionX, 0, MotionZ));
                MotionVertical = Mth.Abs(MotionY);
                Debug.Player = Entity.GetChunkPosition();
            }
            else
            {
                MotionHorizon = MotionVertical = 0;
            }

            // Корректируем для следующего тика

            // трение воздуха
            float study = .91f;
            // Трение с блоком
            //study = (.6f) * .91f;
            // float param = 0.16277136f / (study * study * study);

            // Параметр падение 
            //MotionY -= .16f; // minecraft .08f

            // Трение воздуха
            MotionX *= study;
            //MotionY *= .98f;
            MotionY *= .6f;
            MotionZ *= study;
        }

    }
}
