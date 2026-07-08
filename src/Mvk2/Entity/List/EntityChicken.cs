using Mvk2.Entity.AI;
using Mvk2.Entity.AI.PathFinding;
using Mvk2.Entity.Damage;
using System.Runtime.CompilerServices;
using Vge.Entity;
using Vge.Entity.AI;
using Vge.Entity.AI.PathFinding;
using Vge.Entity.Physics;
using Vge.Entity.Sizes;
using Vge.World;

namespace Mvk2.Entity.List
{
    /// <summary>
    /// Сущность курицы
    /// </summary>
    public class EntityChicken : EntityMob
    {
        public EntityChicken() : base()
        {
            SolidHeadWithBody = false;
           // _persistenceRequired = true;
        }

        /// <summary>
        /// Инициализация размеров сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitSize()
            => Size = SizeLiving = new SizeEntityLiving(this, .3f, .99f, .85f, 6);
            //=> Size = SizeLiving = new SizeEntityLiving(this, .6f, 3.6f, 3.36f, 80);

        /// <summary>
        /// Инициализация физики
        /// </summary>
        protected override void _InitPhysics(CollisionBase collision)
        {
            PhysicsGroundLiving physicsGround = new PhysicsGroundLiving(collision, this);
            physicsGround.SetHeightAutoJump(1f);
            Physics = physicsGround;
            PresenceBlocks = new PresenceBlocksMvk(this);
        }

        /// <summary>
        /// Инициализация навигации
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override PathNavigate _InitNavigate(WorldServer world) 
            => new PathNavigateGround(this, world);

        /// <summary>
        /// Инициализация на сервере, после всех инициализаций
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitServer()
        {
            base._InitServer();
            Damage = new DamageLiving(this);
            Speed = .05f;

            _tasks.AddTask(0, new EntityAISwimming(this));
            _tasks.AddTask(1, new EntityAIPanic(this, 1.6f));
            _tasks.AddTask(1, new EntityAIFindShore(this, 1.2f));
            _tasks.AddTask(4, new EntityAIFollowYour(this, .002f, 1.2f, 32));
            _tasks.AddTask(4, new EntityAIWander(this, .004f));
            _tasks.AddTask(5, new EntityAIWatchClosest(this, 10f));
            _tasks.AddTask(6, new EntityAIIncreaseHealth(this));
            _tasks.AddTask(6, new EntityAILookIdle(this));
        }

        /// <summary>
        /// Игровой такт на клиенте
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void UpdateClient(WorldClient world, float deltaTime)
        {
            UpdatePositionServer();
            // Поворот тела от поворота головы или движения
            _RotationBody();
            Render.UpdateClient(world, deltaTime);
        }

        
        /*
        protected override void _UpdateTest()
        {
            ttt++;

            if (ttt == 45)
            {
                //MoveHelper.SetJumping();
                //ttt = 0;
            }
            if (ttt < 60)
            {
                // Пробегаем по всем сущностям и обрабатываеи их такт
                //for (int i = 0; i < _worldServer.LoadedEntityList.Count; i++)
                //{
                //    EntityBase entity = _worldServer.LoadedEntityList.GetAt(i);

                //    if (entity != null && entity is PlayerServer playerServer)
                //    {
                //        //LookHelper.SetLookPositionWithEntity(playerServer, .125f, .075f);
                //        MoveHelper.SetMoveTo(playerServer.PosX, playerServer.PosY, playerServer.PosZ, 0.25f);
                //        GetWorldServer().Tracker.SendToAllTrackingEntity(Id,
                //            new PacketS0BAnimation(Id, Physics.Movement.Flags));

                //        break;
                //    }
                //}
                // ttt = 0;
            }
            else if (ttt == 90)
            {
                //GetWorldServer().Tracker.SendToAllTrackingEntity(Id,
                //            new PacketS0BAnimation(Id, Physics.Movement.Flags));

                //GetWorldServer().SpawnParticle(EntitiesFXReg.DebugId, 1,
                //    new WinGL.Util.Vector3(PosX, PosY + 1, PosZ), new WinGL.Util.Vector3(0), 0, 6);

                // Пробегаем по всем сущностям и обрабатываеи их такт
                for (int i = 0; i < _worldServer.LoadedEntityList.Count; i++)
                {
                    EntityBase entity = _worldServer.LoadedEntityList.GetAt(i);

                    if (entity != null && entity is PlayerServer playerServer)
                    {
                        //LookHelper.SetLookPositionWithEntity(playerServer, .125f, .075f);
                        //Stopwatch stopwatch = new Stopwatch();
                        //stopwatch.Start();
                        if (Navigator.TryMoveToEntityLiving(playerServer, .5f, true, false, 0))
                        {
                            //Console.WriteLine("Navigator Length:" +
                            //    Navigator.CurrentPath.GetCurrentPathLength()
                            //    + " ms:" + (stopwatch.ElapsedTicks / (float)(Stopwatch.Frequency / 1000f)));
                            
                            // Debug
                            //Navigator Length:18 ms: 0,1035
                            //Navigator Length:27 ms: 0,1309
                            //Navigator Length:14 ms: 0,071
                            //Navigator Length:6 ms: 0,0407
                            //Navigator Length:11 ms: 0,0652
                            //Navigator Length:20 ms: 0,0912
                            //Navigator Length:16 ms: 0,116
                            //Navigator Length:18 ms: 0,083
                            //Navigator Length:18 ms: 0,0838
                            //Navigator Length:31 ms: 0,1444
                            //Navigator Length:31 ms: 0,1161

                            // Release
                            //Navigator Length:22 ms: 0,0412
                            //Navigator Length:14 ms: 0,0256
                            //Navigator Length:21 ms: 0,0345
                            //Navigator Length:7 ms: 0,0179
                            //Navigator Length:18 ms: 0,0299
                            //Navigator Length:10 ms: 0,0195
                            //Navigator Length:12 ms: 0,0388
                            //Navigator Length:10 ms: 0,0348
                            //Navigator Length:14 ms: 0,0361
                            //MoveHelper.SetMoveTo(playerServer.PosX, playerServer.PosY, playerServer.PosZ, 0.25f);

                        }

                        break;
                    }
                }

                

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
        */
    }
    
}
