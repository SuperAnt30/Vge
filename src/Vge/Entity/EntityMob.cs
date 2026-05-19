using System.Runtime.CompilerServices;
using Vge.Entity.AI;
using Vge.Entity.Player;
using Vge.Network.Packets.Server;

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
        /// Объект для вращения сущности AI
        /// </summary>
        public readonly EntityLookHelper LookHelper;
        /// <summary>
        /// Объект для перемещения сущности до координаты AI
        /// </summary>
        private readonly EntityMoveHelper MoveHelper;
        /// <summary>
        /// Объект навигации сущности
        /// </summary>
        //protected PathNavigate _navigator;
        /// <summary>
        /// Пасивные задачи (бродить, смотреть, бездельничать, ...)
        /// </summary>
        //protected EntityAITasks _tasks;

        public EntityMob()
        {
            //Standing();
            //tasks = new EntityAITasks(world);
            //targetTasks = new EntityAITasks(world);
            LookHelper = new EntityLookHelper(this);
            MoveHelper = new EntityMoveHelper(this);
            //navigator = InitNavigate(World);
            ////SpeedSurvival();
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
                //navigator.OnUpdateNavigation();
                // Задачи моба. 
                //UpdateAITasks();
                // Перемещение. Вращение моба по вектору перемещения, и определяется длинна шага
                MoveHelper.OnUpdateMove();
                // Смотрит.
                LookHelper.OnUpdateLook();

                // _UpdateEntityActionState();
                _worldServer.Filer.EndSection();
            }

            if (!Physics.IsPhysicSleep())
            {
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

            ttt++;

            if (ttt == 45)
            {
                //MoveHelper.SetJumping();
                //ttt = 0;
            }
            if (ttt < 60)
            {
                // Пробегаем по всем сущностям и обрабатываеи их такт
                for (int i = 0; i < _worldServer.LoadedEntityList.Count; i++)
                {
                    EntityBase entity = _worldServer.LoadedEntityList.GetAt(i);

                    if (entity != null && entity is PlayerServer playerServer)
                    {
                        //LookHelper.SetLookPositionWithEntity(playerServer, .125f, .075f);
                        MoveHelper.SetMoveTo(playerServer.PosX, playerServer.PosY, playerServer.PosZ, 0.25f);
                        GetWorldServer().Tracker.SendToAllTrackingEntity(Id,
                            new PacketS0BAnimation(Id, Physics.Movement.Flags));

                        break;
                    }
                }
                // ttt = 0;
            }
            else if (ttt == 61)
            {
                GetWorldServer().Tracker.SendToAllTrackingEntity(Id,
                            new PacketS0BAnimation(Id, Physics.Movement.Flags));
            }
            else if (ttt > 120)
            {
                ttt = 0;
                //GetWorldServer().Tracker.SendToAllTrackingEntity(Id,
                //            new PacketS0BAnimation(Id, Physics.Movement.Flags));
            }


            //if (ttt == 30)
            //{
            //    RotationYaw += .4f;
            //   // Physics.AwakenPhysics();
            //}
            //else if (ttt == 60)
            //{
            //    RotationYaw -= .8f;
            // //   Physics.AwakenPhysics();
            //    ttt = 0;
            //}
        }

        private int ttt = 0;
    }
}
