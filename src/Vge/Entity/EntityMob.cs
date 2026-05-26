using System.Runtime.CompilerServices;
using Vge.Entity.AI;
using Vge.Entity.AI.PathFinding;
using Vge.Network.Packets.Server;
using Vge.World;

namespace Vge.Entity
{
    /// <summary>
    /// Объект для мобов сущности которые имеют AI, которая может сама перемещаться и вращаться.
    /// </summary>
    public abstract class EntityMob : EntityLiving
    {
        /// <summary>
        /// Специфический возраст этой сущности, чем ближе к игроку возраст обнуляется, 
        /// чем дальше растёт, для вероятности диспавна
        /// </summary>
        public int EntityAge { get; protected set; } = 0;

        /// <summary>
        /// Максимальная высота, с которой объекту разрешено прыгать (используется в навигаторе)
        /// </summary>
        public int MaxFallHeight { get; protected set; } = 1;

        /// <summary>
        /// Объект для вращения сущности AI
        /// </summary>
        public EntityLookHelper LookHelper { get; protected set; }
        /// <summary>
        /// Объект для перемещения сущности до координаты AI
        /// </summary>
        public EntityMoveHelper MoveHelper { get; protected set; }
        /// <summary>
        /// Объект навигации сущности
        /// </summary>
        public PathNavigate Navigator { get; protected set; }
        /// <summary>
        /// Пасивные задачи (бродить, смотреть, бездельничать, ...)
        /// </summary>
        //protected EntityAITasks _tasks;

        /// <summary>
        /// Инициализация навигации
        /// </summary>
        protected virtual PathNavigate _InitNavigate(WorldServer world) => null;

        /// <summary>
        /// Флаг данных общего перемещения прошлого такта
        /// FBLRSnJSp
        /// </summary>
        private byte _flagMovement;

        /// <summary>
        /// Инициализация на сервере, после всех инициализаций
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitServer()
        {
            //tasks = new EntityAITasks(world);
            //targetTasks = new EntityAITasks(world);
            LookHelper = new EntityLookHelper(this);
            MoveHelper = new EntityMoveHelper(this);
            Navigator = _InitNavigate(GetWorldServer());
            //SpeedSurvival();
        }

        /// <summary>
        /// Задать вращение для сущностей с AI
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRotationAI(float yaw, float pitch)
        {
            RotationYaw = yaw;
            RotationPitch = pitch;
            _RotationBody();
            Physics?.AwakenPhysics();
        }

        /// <summary>
        /// Игровой такт на сервере
        /// </summary>
        public override void Update()
        {
            if (!_IsMovementBlocked())
            {
                // ИИ
                _worldServer.Filer.StartSection("Ai");
                EntityAge++;

                // Диспавн моба.
                //DespawnEntity();
                // Ощущение. (senses)

                // Определение цели
                //tasks.OnUpdateTasks();
                // Навигация. Расчёт следующего шага до точки прибытия в навигации
                Navigator.OnUpdateNavigation();
                // Задачи моба. 
                //UpdateAITasks();
                // Перемещение. Вращение моба по вектору перемещения, и определяется длинна шага
                MoveHelper.OnUpdateMove();
                // Смотрит.
                LookHelper.OnUpdateLook();

                // _UpdateEntityActionState();
                _worldServer.Filer.EndSection();

                // Анимация движения
                if (_flagMovement != Physics.Movement.Flags)
                {
                    GetWorldServer().Tracker.SendToAllTrackingEntity(Id,
                        new PacketS0BAnimation(Id, Physics.Movement.Flags));
                    _flagMovement = Physics.Movement.Flags;
                }
            }

            if (!Physics.IsPhysicSleep())
            {
                // Обновить наличие блоков в каких находится игрок
                UpdatePresenceBlocks();
                // Расчитать перемещение в объекте физика
                Physics.LivingUpdate();

                //if (Physics.CaughtInBlock > 2 || Physics.CaughtInEntity > 30)
                //{
                //    SetDead();
                //    return;
                //}

                if (IsPositionChange())
                {
                    UpdatePositionPrev();
                    LevelMotionChange = 2;
                }
                else if (!Physics.IsPhysicSleep())
                {
                    // Сущность не спит!
                    // Бодрый, только помечен на пробуждение, но не было перемещения [3 или 4]
                    // Надо ещё 2 такта как минимум передать, чтоб клиент, зафиксировал сон [1]
                    LevelMotionChange = 2;
                }
            }
            _UpdateTest();
        }

        protected virtual void _UpdateTest() { }
    }
}
