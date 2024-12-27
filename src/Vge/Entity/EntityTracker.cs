using System;
using Vge.Games;
using Vge.Management;
using Vge.Network;
using Vge.World;
using WinGL.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Объект прослеживания всех видимых сущностей на сервере
    /// </summary>
    public class EntityTracker
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
        /// Список всех треков сущностей
        /// </summary>
        private readonly MapEntityTrackerEntry _trackedEntities = new MapEntityTrackerEntry();

        /// <summary>
        /// Максимальное пороговое значение расстояния отслеживания 
        /// </summary>
        private readonly int _maxTrackingDistanceThreshold = 512;

        public EntityTracker(WorldServer world)
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
                _AddEntityToTracker(entity, 256);

                EntityTrackerEntry trackerEntry;
                for (int i = 0; i < _trackedEntities.Count; i++)
                {
                    trackerEntry = _trackedEntities.GetAt(i);
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
            //else
            //{
            //    AddEntityToTracker(entity, 128, 10, false);
            //}
        }

        /// <summary>
        /// Убрать трек с этой сущностью
        /// </summary>
        public void UntrackEntity(EntityBase entity)
        {
            EntityTrackerEntry trackerEntry;

            if (entity is PlayerServer playerServer)
            {
                for (int i = 0; i < _trackedEntities.Count; i++)
                {
                    trackerEntry = _trackedEntities.GetAt(i);
                    if (trackerEntry != null)
                    {
                        trackerEntry.RemoveTrackedPlayerSymmetric(playerServer);
                    }
                }
            }

            trackerEntry = _trackedEntities.Get(entity.Id);

            if (trackerEntry != null)
            {
                _trackedEntities.Remove(trackerEntry);
                trackerEntry.DestroyEntityPacketToTrackedPlayers();
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
            EntityTrackerEntry entityTracker = _trackedEntities.Get(entity.Id);
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
            EntityTrackerEntry entityTracker = _trackedEntities.Get(entity.Id);
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
                EntityTrackerEntry trackerEntry = _trackedEntities.GetAt(i);
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
                    World.Server.Log.Server("EntityTracker: Сущность {0} уже отслеживается!", entity.GetType());
                    return;
                }
                EntityTrackerEntry trackerEntry = new EntityTrackerEntry(entity, trackingRange);
                _trackedEntities.Add(trackerEntry);
            }
            catch (Exception ex)
            {
                World.Server.Log.Error("EntityTracker: Обнаружение ошибки отслеживания объекта: " + ex.Message);
            }
        }
    }
}
