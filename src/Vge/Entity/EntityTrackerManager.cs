using System;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Network;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Объект прослеживания всех видимых сущностей на сервере
    /// </summary>
    public class EntityTrackerManager
    {
        /// <summary>
        /// Сетевой мир
        /// </summary>
        public readonly WorldServer World;
        /// <summary>
        /// Основной сервер
        /// </summary>
        public readonly GameServer Server;

        /// <summary>
        /// Карта всех треков сущностей вместе с игроками
        /// EntityTracker
        /// </summary>
        private readonly MapEntity<EntityTracker> _trackedEntities = new MapEntity<EntityTracker>();
        /// <summary>
        /// Карта всех треков игроков
        /// EntityTracker но нужен PlayerServer
        /// </summary>
        private readonly MapEntity<PlayerServer> _trackedPlayers = new MapEntity<PlayerServer>();

        /// <summary>
        /// Максимальное пороговое значение расстояния отслеживания 
        /// </summary>
        private readonly int _maxTrackingDistanceThreshold = 512;

        public EntityTrackerManager(WorldServer world)
        {
            World = world;
            Server = World.Server;
        }

        /// <summary>
        /// Добавить сущность
        /// </summary>
        /// <param name="entity">сущность</param>
        public void EntityAdd(EntityBase entity)
        {
            if (entity is PlayerServer playerServer)
            {
                _AddEntityToTracker(entity, 64);// 256);

                EntityTracker trackerEntry;
                for (int i = 0; i < _trackedEntities.Count; i++)
                {
                    trackerEntry = _trackedEntities.GetAt(i) as EntityTracker;
                    if (trackerEntry != null)
                    {
                        trackerEntry.UpdatePlayerEntity(playerServer);
                    }
                }
            }
            //else if (entity is EntityItem)
            //{
            //    AddEntityToTracker(entity, 64, 20, true); // item
            //}
            else
            {
                _AddEntityToTracker(entity, 128);
            }
        }

        /// <summary>
        /// Убрать трек с этой сущностью
        /// </summary>
        public void UntrackEntity(EntityBase entity)
        {
            EntityTracker trackerEntry;

            if (entity is PlayerServer playerServer)
            {
                for (int i = 0; i < _trackedEntities.Count; i++)
                {
                    trackerEntry = _trackedEntities.GetAt(i) as EntityTracker;
                    if (trackerEntry != null)
                    {
                        trackerEntry.RemoveTrackedPlayerSymmetric(playerServer);
                    }
                }
            }

            trackerEntry = _trackedEntities.Get(entity.Id) as EntityTracker;

            if (trackerEntry != null)
            {
                _trackedEntities.Remove(trackerEntry.Id, trackerEntry);
                _trackedPlayers.Remove(trackerEntry.Id, trackerEntry.TrackedEntity as PlayerServer);
                trackerEntry.DestroyEntityPacketToTrackedPlayers();
            }
        }

        /// <summary>
        /// Обновить все отслеживаемые сущности
        /// </summary>
        public void Update()
        {
            EntityTracker entityTracker;
            EntityTracker entityTracker2;
            for (int i = 0; i < _trackedEntities.Count; i++)
            {
                entityTracker = _trackedEntities.GetAt(i) as EntityTracker;
                if (entityTracker != null)
                {
                    entityTracker.UpdatePlayerList(_trackedPlayers);

                    if (entityTracker.FlagUpdatePlayerEntity && entityTracker.TrackedEntity is PlayerServer playerServer)
                    {
                        for (int j = 0; j < _trackedEntities.Count; j++)
                        {
                            entityTracker2 = _trackedEntities.GetAt(j) as EntityTracker;
                            if (entityTracker2 != null && entityTracker2.TrackedEntity.Id != playerServer.Id)
                            {
                                entityTracker2.UpdatePlayerEntity(playerServer);
                            }
                        }
                    }
                }
            }
        }

        #region Send

        /// <summary>
        /// Отправить всем отслеживаемым игрока пакет, кроме тикущей
        /// </summary>
        /// <param name="entity">сущность</param>
        /// <param name="packet">пакет</param>
        public void SendToAllTrackingEntity(EntityBase entity, IPacket packet)
        {
            EntityTracker entityTracker = _trackedEntities.Get(entity.Id) as EntityTracker;
            if (entityTracker != null)
            {
                entityTracker.SendPacketPlayers(packet);
            }
        }

        /// <summary>
        /// Отправить всем отслеживаемым игрока пакет, с тикущей
        /// </summary>
        /// <param name="entity">сущность</param>
        /// <param name="packet">пакет</param>
        public void SendToAllTrackingEntityCurrent(EntityBase entity, IPacket packet)
        {
            EntityTracker entityTracker = _trackedEntities.Get(entity.Id) as EntityTracker;
            if (entityTracker != null)
            {
                entityTracker.SendPacketPlayersCurrent(packet);
            }
        }

        /// <summary>
        /// Отправить пакет игрокам которые в радиуси выбранной позиции
        /// </summary>
        public void SendToAllEntityDistance(Vector3 pos, float distance, IPacket packet)
        {
            for (int i = 0; i < _trackedEntities.Count; i++)
            {
                EntityTracker trackerEntry = _trackedEntities.GetAt(i) as EntityTracker;
                if (trackerEntry != null && trackerEntry.TrackedEntity is PlayerServer playerServer)
                {
                    if (playerServer.Distance(pos) < distance)
                    {
                        playerServer.SendPacket(packet);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Добавить сущность в трек
        /// </summary>
        /// <param name="entity">сущность</param>
        /// <param name="trackingRange">Пороговое значение расстояния отслеживания</param>
        private void _AddEntityToTracker(EntityBase entity, int trackingRange)
        {
            if (trackingRange > _maxTrackingDistanceThreshold)
            {
                trackingRange = _maxTrackingDistanceThreshold;
            }

            try
            {
                if (_trackedEntities.ContainsId(entity.Id))
                {
                    World.Server.Log.Server(Srl.EntityAlreadyBeingTracked, entity.GetType());
                    return;
                }
                EntityTracker trackerEntry = new EntityTracker(entity, trackingRange);
                _trackedEntities.Add(trackerEntry.Id, trackerEntry);
                if (entity is PlayerServer playerServer)
                {
                    _trackedPlayers.Add(playerServer.Id, playerServer);
                }
            }
            catch (Exception ex)
            {
                World.Server.Log.Error(Srl.DetectingObjectTrackingError, ex.Message);
            }
        }

        public override string ToString()
        {
            //string list = "";
            //for (int i = 0; i < _trackedEntities.Count; i++)
            //{
            //    list += "[" + _trackedEntities.GetAt(i).ToString() + "] ";
            //}
            // Считаем сколько сущностей не спят
            int countPhysics = 0;
            EntityBase entity;
            for (int i = 0; i < _trackedEntities.Count; i++)
            {
                entity = _trackedEntities.GetAt(i).TrackedEntity;
                if (entity.Physics != null && !entity.Physics.IsPhysicSleep())
                {
                    countPhysics++;
                }
            }
            return "Tracker: " + _trackedEntities.Count + " Physics: " + countPhysics;
        }
    }
}
