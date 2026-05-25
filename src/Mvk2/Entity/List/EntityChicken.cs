using Mvk2.Entity.AI.PathFinding;
using System;
using System.Diagnostics;
using Vge.Entity;
using Vge.Entity.AI.PathFinding;
using Vge.Entity.Particle;
using Vge.Entity.Physics;
using Vge.Entity.Player;
using Vge.Entity.Sizes;
using Vge.Network.Packets.Server;
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
            _persistenceRequired = true;
        }

        /// <summary>
        /// Инициализация размеров сущности
        /// </summary>
        protected override void _InitSize()
            => Size = new SizeEntityLiving(this, .3f, .99f, .85f, 6);

        /// <summary>
        /// Инициализация физики
        /// </summary>
        protected override void _InitPhysics(CollisionBase collision)
        {
            PhysicsGroundLiving physicsGround = new PhysicsGroundLiving(collision, this);
            physicsGround.SetHeightAutoJump(1f);
            Physics = physicsGround;
            PresenceBlocks = new PresenceBlocksMvk();
        }

        /// <summary>
        /// Инициализация навигации
        /// </summary>
        protected override PathNavigate _InitNavigate(WorldServer world) 
            => new PathNavigateGround(this, world);

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

                            //MoveHelper.SetMoveTo(playerServer.PosX, playerServer.PosY, playerServer.PosZ, 0.25f);
                            GetWorldServer().Tracker.SendToAllTrackingEntity(Id,
                                new PacketS0BAnimation(Id, Physics.Movement.Flags));
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
    }
}
