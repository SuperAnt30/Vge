using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Network;
using Vge.Network.Packets.Server;
using Vge.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Объект прослеживания конкретной сущности
    /// </summary>
    public class EntityTracker
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
        /// Флаг надо ли запускать обновление UpdatePlayerEntity
        /// </summary>
        public bool FlagUpdatePlayerEntity { get; private set; }
        
        /// <summary>
        /// Счётчитк обновлений конкретной сущности
        /// </summary>
        public int UpdateCounter { get; private set; }

        public int Id => TrackedEntity.Id;

        /// <summary>
        /// Содержит ссылки на всех игроков, которые в настоящее время получают обновления позиций для этого объекта.
        /// </summary>
        private readonly ListMessy<PlayerServer> _trackingPlayers = new ListMessy<PlayerServer>();

        private float _lastTrackedEntityPosX;
        private float _lastTrackedEntityPosY;
        private float _lastTrackedEntityPosZ;

        public EntityTracker(EntityBase entity, int trackingRange)
        {
            TrackedEntity = entity;
            TrackingDistanceThreshold = trackingRange;
        }

        /// <summary>
        /// Обновить список игроков
        /// </summary>
        //public void UpdatePlayerEntities(MapListEntity entityPlayers)
        //{
        //    for (int i = 0; i < entityPlayers.Count; i++)
        //    {
        //        UpdatePlayerEntity((EntityPlayerServer)entityPlayers.GetAt(i));
        //    }
        //}

        /// <summary>
        /// Обновить видна ли текущая сущность у игрока playerServer
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
        /// Обновить видимость сущностей с списком игроков
        /// </summary>
        /// <param name="playerEntities">список игроков</param>
        public void UpdatePlayerList(MapEntity<PlayerServer> trackedPlayers)
        {
            FlagUpdatePlayerEntity = false;

            if (TrackedEntity.IsDistanceCube(_lastTrackedEntityPosX, _lastTrackedEntityPosY, _lastTrackedEntityPosZ, 2)
                || TrackedEntity.IsOverviewChunkChanged())
            {
                _lastTrackedEntityPosX = TrackedEntity.PosX;
                _lastTrackedEntityPosY = TrackedEntity.PosY;
                _lastTrackedEntityPosZ = TrackedEntity.PosZ;
                FlagUpdatePlayerEntity = true;
                if (TrackedEntity.IsOverviewChunkChanged())
                {
                    TrackedEntity.MadeOverviewChunkChanged();
                }
                for (int i = 0; i < trackedPlayers.Count; i++)
                {
                    UpdatePlayerEntity(trackedPlayers.GetAt(i));
                }
            }
                
            if (TrackedEntity.LevelMotionChange > 0)
            {
                SendPacketPlayers(new PacketS14EntityMotion(TrackedEntity));
                TrackedEntity.LevelMotionChange--;
            }

            //else if (TrackedEntity is EntityItem entityItem)
            //{
            //    if (entityItem.IsMoving)
            //    {
            //        SendPacketPlayers(new PacketS14EntityMotion(entityItem));
            //    }
            //}
            //else 
            //if (TrackedEntity is EntityThrowable entityThrowable)
            //{
            //    SendPacketPlayers(new PacketS14EntityMotion(entityThrowable));
            //}

            if (TrackedEntity.MetaData.IsChanged) //UpdateCounter % UpdateFrequency == 0)
            {
                SendPacketPlayersCurrent(new PacketS1CEntityMetadata(
                    TrackedEntity.Id, TrackedEntity.MetaData.GetChanged()));
            }

            UpdateCounter++;
        }

        /// <summary>
        /// Определяем кого надо спавнить
        /// </summary>
        private IPacket _PacketSpawn()
        {
            if (TrackedEntity is PlayerServer playerServer)
            {
                return new PacketS0CSpawnPlayer(playerServer);
            }
            else
            {
                return new PacketS0FSpawnMob(TrackedEntity);
            }
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
            => obj is EntityTracker entityTracker ? entityTracker.TrackedEntity.Id == TrackedEntity.Id : false;

        public override int GetHashCode() => TrackedEntity.Id;

        public override string ToString()
        {
            string list = "";
            for (int i = 0; i < _trackingPlayers.Count; i++)
            {
                list += _trackingPlayers[i].Id + ", ";
            }
            return string.Format("#{0} {1} c:{2} ({3})", 
                TrackedEntity.Id, TrackedEntity.GetName(), _trackingPlayers.Count, list);
        }
    }
}
