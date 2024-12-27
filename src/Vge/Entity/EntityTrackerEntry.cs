using System.Runtime.CompilerServices;
using Vge.Management;
using Vge.Network;
using Vge.Network.Packets.Server;
using Vge.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Объект прослеживания конкретной сущности
    /// </summary>
    public class EntityTrackerEntry
    {
        /// <summary>
        /// Объект сущности которую прослеживаем
        /// </summary>
        public readonly EntityBase TrackedEntity;
        /// <summary>
        /// Пороговое значение расстояния отслеживания
        /// </summary>
        public readonly int TrackingDistanceThreshold;
        /// <summary>
        /// ****
        /// </summary>
        public bool PlayerEntitiesUpdated { get; private set; }
        /// <summary>
        /// Счётчитк обновлений
        /// </summary>
        public int UpdateCounter { get; private set; }

        /// <summary>
        /// Содержит ссылки на всех игроков, которые в настоящее время получают обновления позиций для этого объекта.
        /// </summary>
        private readonly ListMessy<PlayerServer> _trackingPlayers = new ListMessy<PlayerServer>();

        private float _lastTrackedEntityPosX;
        private float _lastTrackedEntityPosY;
        private float _lastTrackedEntityPosZ;

        public EntityTrackerEntry(EntityBase entity, int trackingRange)
        {
            TrackedEntity = entity;
            TrackingDistanceThreshold = trackingRange;
        }

        /// <summary>
        /// Обновить видна ли текущая сущность у игрока entityPlayer
        /// </summary>
        public void UpdatePlayerEntity(PlayerServer playerServer)
        {
            if (playerServer != TrackedEntity)
            {
                if (_CheckPosition(playerServer))
                {
                    if (!_trackingPlayers.Contains(playerServer))
                    {
                        _trackingPlayers.Add(playerServer);
                        IPacket packet = _PacketSpawn();
                        playerServer.SendPacket(packet);
                    }
                }
                else if (_trackingPlayers.Contains(playerServer))
                {
                    _trackingPlayers.Remove(playerServer);
                    playerServer.SendRemoveEntity(TrackedEntity);
                }
            }
        }

        /// <summary>
        /// Определяем кого надо спавнить
        /// </summary>
        private IPacket _PacketSpawn()
        {
            //if ((TrackedEntity.GetEntityType() == EnumEntities.Player || TrackedEntity.GetEntityType() == EnumEntities.PlayerInvisible)
            //    && TrackedEntity is EntityPlayerServer entityPlayerServer)
            //{
                return new PacketS0CSpawnPlayer((PlayerServer)TrackedEntity);
            //}
            //else
            //{
            //    return new PacketS0FSpawnMob(TrackedEntity);
            //}
        }

        #region Send

        /// <summary>
        /// Отправить сетевой пакет всем игрокам которые видят эту сущность без этой
        /// </summary>
        public void SendPacketPlayers(IPacket packet)
        {
            for (int i = 0; i < _trackingPlayers.Count; i++)
            {
                _trackingPlayers[i].SendPacket(packet);
            }
        }

        /// <summary>
        /// Отправить сетевой пакет всем игрокам которые видят эту сущность вместе с этой
        /// </summary>
        public void SendPacketPlayersCurrent(IPacket packet)
        {
            SendPacketPlayers(packet);
            if (TrackedEntity is PlayerServer playerServer)
            {
                playerServer.SendPacket(packet);
            }
        }

        #endregion

        /// <summary>
        /// Удалите отслеживаемого игрока из нашего списка и 
        /// прикажите отслеживаемому игроку уничтожить нас из своего мира. 
        /// </summary>
        public void RemoveTrackedPlayerSymmetric(PlayerServer playerServer)
        {
            if (_trackingPlayers.Contains(playerServer))
            {
                _trackingPlayers.Remove(playerServer);
                playerServer.SendRemoveEntity(TrackedEntity);
            }
        }


        /// <summary>
        /// Удалить у всех игроков, тикущую отслеживаемую сущность
        /// </summary>
        public void DestroyEntityPacketToTrackedPlayers()
        {
            for (int i = 0; i < _trackingPlayers.Count; i++)
            {
                _trackingPlayers[i].SendRemoveEntity(TrackedEntity);
            }
        }

        /// <summary>
        /// Проверка позиции
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _CheckPosition(PlayerServer playerServer)
        {
            float c = playerServer.OverviewChunk << 4;
            if (c > TrackingDistanceThreshold) c = TrackingDistanceThreshold;
            return TrackedEntity.Distance(playerServer) < c;
        }

        public override bool Equals(object obj)
            => obj is EntityTrackerEntry entityTracker ? entityTracker.TrackedEntity.Id == TrackedEntity.Id : false;

        public override int GetHashCode() => TrackedEntity.Id;
    }
}
