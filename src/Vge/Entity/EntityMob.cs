using System.Runtime.CompilerServices;
using Vge.Entity.AI;
using Vge.Entity.AI.PathFinding;
using Vge.Entity.Player;
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
        public int EntityAge = 0;

        /// <summary>
        /// Максимальная высота, с которой объекту разрешено прыгать (используется в навигаторе)
        /// </summary>
        public int MaxFallHeight { get; protected set; } = 1;

        /// <summary>
        /// Сколько тиков эта сущность прожила
        /// </summary>
        public int TicksExisted { get; protected set; } = 0;
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
        protected EntityAITasks _tasks;

        /// <summary>
        /// Получает активную цель, которую система задач использует для отслеживания
        /// </summary>
        public EntityLiving AttackTarget;
        /// <summary>
        /// Последний атакующий
        /// </summary>
        private EntityLiving _lastAttacker;
        /// <summary>
        /// Содержит значение TicksExisted при последнем вызове SetLastAttacker
        /// </summary>
        private int _lastAttackerTime;

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
            _tasks = new EntityAITasks();
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
        /// Возвращает true, если другие Сущности не должны проходить через эту Сущность
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanBeCollidedWith() => true;

        /// <summary>
        /// Определяет, может ли объект исчезнуть, использовать его на бездействующих удаленных объектах.
        /// К примеру монстр с предметом уже не исчезнет
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool _CanDespawn() => true;

        /// <summary>
        /// Игровой такт на сервере
        /// </summary>
        public override void Update()
        {
            TicksExisted++;
            // Обновление урона
            Damage?.Update();

            if (!_IsMovementBlocked())
            {
                // ИИ
                _worldServer.Filer.StartSection("Ai " + Ce.Entities.EntitiesAlias[IndexEntity]);
                EntityAge++;

                // Диспавн моба.
                _DespawnEntity();
                // Ощущение. (senses)

                // Определение цели
                _tasks.OnUpdateTasks();
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
                UpdatePresenceBlocks(_worldServer);
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
        }


        /// <summary>
        /// Заставляет сущность исчезать, если требования выполнены
        /// </summary>
        private void _DespawnEntity()
        {
            if (_persistenceRequired && !_CanDespawn())
            {
                EntityAge = 0;
            }
            else
            {
                PlayerServer entityPlayer = GetWorldServer().GetClosestPlayerToEntity(this, -1f);
                if (entityPlayer != null)
                {
                    float k = entityPlayer.DistanceSqToEntity(this);
                    // 16384 это растояние примерно 128
                    // 25600 это растояние примерно 160
                    if (k > 25600)
                    {
                        SetDead();
                    }
                    // 1024 это растояние примерно 32
                    // 2304 это растояние примерно 48
                    if (EntityAge > 600 && k > 2304 && GetWorldServer().Rnd.Next(800) == 0)
                    {
                        SetDead();
                    }

                    if (k < 2304)
                    {
                        EntityAge = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Сущность которая последняя атаковала
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityLiving GetAITarget() => _lastAttacker != null
            && TicksExisted - _lastAttackerTime < 400 ? _lastAttacker : null;

        /// <summary>
        /// Задать сущность которая последняя атаковала
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLastAttacker(EntityLiving entity)
        {
            _lastAttacker = entity;
            _lastAttackerTime = TicksExisted;
        }

        /// <summary>
        /// Обновить время агрессии на текущую сущность
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpLastAttacker() => _lastAttackerTime = TicksExisted;

        /// <summary>
        /// Будет уничтожен следующим тиком
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetDead()
        {
            base.SetDead();
            // Тут надо пробежаться по всем сущностям и оповестить о смерти
            GetWorldServer()?.AboutAllEntitesToDead(this);
        }

        /// <summary>
        /// Объявить сущности о смерте тикущей сущности
        /// </summary>
        public override void AboutToDead(EntityMob entityMob)
        {
            if (AttackTarget != null && AttackTarget.Id == entityMob.Id)
            {
                AttackTarget = null;
            }
            if (_lastAttacker != null && _lastAttacker.Id == entityMob.Id)
            {
                _lastAttacker = null;
            }
        }
    }
}
