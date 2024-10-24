﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Vge.Games;
using Vge.NBT;
using Vge.Network;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;
using Vge.World;
using Vge.World.Chunk;
using WinGL.Util;

namespace Vge.Management
{
    /// <summary>
    /// Объект игрока, как локального так и сетевого, не сущность
    /// </summary>
    public class PlayerServer : PlayerBase, IAnchor
    {
        /// <summary>
        /// Сетевой сокет
        /// </summary>
        public readonly SocketSide Socket;
        /// <summary>
        /// Флаг является ли этот игрок владельцем
        /// </summary>
        public readonly bool Owner;
        /// <summary>
        /// Указать причину удаления
        /// </summary>
        public string causeRemove = "";
        /// <summary>
        /// Сколько мили секунд эта сущность прожила
        /// </summary>
        public double TimesExisted { get; private set; }

        #region Anchor 

        /// <summary>
        /// Является ли якорь активным
        /// </summary>
        public bool IsActive => true;
        /// <summary>
        /// Активный радиус обзора для сервера, нужен для спавна и тиков блоков
        /// </summary>
        public byte ActiveRadius => GetWorld().Settings.ActiveRadius;
        /// <summary>
        /// Является ли якорь игроком
        /// </summary>
        public bool IsPlayer => true;

        /// <summary>
        /// Координату X в каком чанке находится
        /// </summary>
        public int ChunkPositionX => chPos.X;
        /// <summary>
        /// Координата Y в каком чанке находится
        /// </summary>
        public int ChunkPositionY => chPos.Y;

        /// <summary>
        /// В какой позиции X чанка было обработка видимых чанков
        /// </summary>
        public int ChunkPosManagedX { get; private set; }
        /// <summary>
        /// В какой позиции Y чанка было обработка видимых чанков
        /// </summary>
        public int ChunkPosManagedY { get; private set; }

        /// <summary>
        /// Список координат чанков для загрузки, формируется по дистанции к игроку.
        /// Список пополняется при перемещении и уменьшается при проверке, что чанк загружен.
        /// Когда все загружены должно быть 0.
        /// </summary>
        private ListFast<ulong> _loadingChunksSort = new ListFast<ulong>();
        /// <summary>
        /// Список всех ChunkPosition которые сервер должен загрузить возле игрока,
        /// Корректируется от перемещения, используется до сортировки.
        /// После сортирует в _loadingChunksSort.
        /// Когда все загружены должно быть 0
        /// </summary>
        private readonly MapChunk _loadingChunks = new MapChunk();

        /// <summary>
        /// Список координат видимых чанков для игрока, формируется по дистанции к игроку.
        /// Список пополняется при перемещении и уменьшается при проверке, что чанк загружен.
        /// Когда все отправлены должно быть 0
        /// </summary>
        public ListFast<ulong> _clientChunksSort { get; private set; } = new ListFast<ulong>();
        /// <summary>
        /// Список всех ChunkPosition которые видит игрок, сервер отправляет через пакеты.
        /// Корректируется от перемещения, должен быть на 5 по обзору меньше _loadingChunks.
        /// После сортирует в _clientChunksSort.
        /// </summary>
        private readonly MapChunk _clientChunks = new MapChunk();

        /// <summary>
        /// Массив для перезаписи
        /// </summary>
        private ListFast<ulong> _loadedNull = new ListFast<ulong>(10);
        /// <summary>
        /// Желаемый размер партии закачки чанков
        /// </summary>
        private byte _desiredBatchSize;

        #endregion

        /// <summary>
        /// Основной сервер
        /// </summary>
        private readonly GameServer server;
        /// <summary>
        /// Имя пути к папке игрока
        /// </summary>
        private readonly string pathName;

        /// <summary>
        /// Создать сетевого
        /// </summary>
        public PlayerServer(string login, string token, SocketSide socket, GameServer server)
        {
            Login = login;
            Token = GetHash(token);
            Socket = socket;
            this.server = server;
            UUID = GetHash(login);
            pathName = server.Settings.PathPlayers + UUID + ".dat";
            Owner = socket == null;
            _desiredBatchSize = Owner ? Ce.MaxDesiredBatchSize : Ce.MinDesiredBatchSize;
            Id = server.LastEntityId();
            _lastTimeServer = server.Time();
        }

        /// <summary>
        /// Создать владельца
        /// </summary>
        public PlayerServer(string login, string token, GameServer server)
            : this(login, token, null, server) { }


        /// <summary>
        /// Получить время в милисекундах с сервера
        /// </summary>
        protected override long _Time() => server.Time();

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void Update()
        {
            // Добавить время к игроку
            TimesExisted += server.DeltaTime;

            // Тут надо анализ сделать было ли перемещение
            if (isPos)
            {
                //server.Filer.StartSection("Fragment" + OverviewChunk);
                GetWorld().Fragment.UpdateMountedMovingAnchor(this);
                //server.Filer.EndSectionLog();
                isPos = false;
            }

            _UpdatePlayer();
        }

        /// <summary>
        /// Отправить сетевой пакет этому игроку
        /// </summary>
        public void SendPacket(IPacket packet)
            => server.ResponsePacket(Socket, packet);

        #region Packet

        /// <summary>
        /// Пакет: Параметры игрока
        /// </summary>
        public void PacketPlayerSetting(PacketC15PlayerSetting packet)
        {
            SetOverviewChunk(packet.OverviewChunk);
            isPos = true;
        }
        
        /// <summary>
        /// Пакет: Подтверждение фрагментов
        /// </summary>
        public void PacketAcknowledgeChunks(PacketC20AcknowledgeChunks packet)
        {
            _desiredBatchSize = Sundry.RecommendedQuantityBatch(packet.Time, packet.Quantity, _desiredBatchSize);
        }

        #endregion

        /// <summary>
        /// Пройдены все проверки, отправляем нужные пакеты игроку
        /// </summary>
        public void JoinGame()
        {
            SendPacket(new PacketS03JoinGame(Id, UUID));
            SendPacket(new PacketS04TimeUpdate(server.TickCounter));
            SendPacket(new PacketS07RespawnInWorld(IdWorld, GetWorld().Settings));
            SendPacket(new PacketS08PlayerPosLook(new Vector3(chPos.X, chPos.Y, 0), 0, 0));
            // И другие пакеты, такие как позиция и инвентарь и прочее


            // Установленный перемещенный якорь
            MountedMovedAnchor();

            // Вносим в менеджер фрагментов игрока
            GetWorld().Fragment.AddAnchor(this);
            
            isPos = true;
        }

        /// <summary>
        /// Игрок покидает игру
        /// </summary>
        public void LeftGame()
        {
            GetWorld().Fragment.RemoveAnchor(this);
            // Сохраняем
            WriteToFile();
        }

        /// <summary>
        /// Получить мир в котором находится игрок
        /// </summary>
        public WorldServer GetWorld() => server.Worlds.GetWorld(IdWorld);

        /// <summary>
        /// Смена мира, передаём новый id мира
        /// </summary>
        public void ChangeWorld(byte newIdWorld)
        {
            GetWorld().Fragment.RemoveAnchor(this);
            // Смена id мира
            IdWorld = newIdWorld;
            SendPacket(new PacketS07RespawnInWorld(IdWorld, GetWorld().Settings));
            // Вносим в менеджер фрагментов игрока
            GetWorld().Fragment.AddAnchor(this);
            // Установленный перемещенный якорь
            MountedMovedAnchor();
        }

        /// <summary>
        /// Получить хэш по строке
        /// </summary>
        public static string GetHash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        #region WriteRead

        /// <summary>
        /// Прочесть данные игрока с файла, возращает true если файл существола
        /// </summary>
        public bool ReadFromFile()
        {
            if (File.Exists(pathName))
            {
                try
                {
                    TagCompound nbt = NBTTools.ReadFromFile(pathName, true);
                    Token = nbt.GetString("Token");
                    TimesExisted = nbt.GetLong("TimesExisted");
                    IdWorld = nbt.GetByte("IdWorld");
                    chPos = new Vector2i(nbt.GetInt("ChX"), nbt.GetInt("ChY"));
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Записать данные игрока в файл
        /// </summary>
        public void WriteToFile()
        {
            GameFile.CheckPath(server.Settings.PathPlayers);
            TagCompound nbt = new TagCompound();
            nbt.SetString("Token", Token);
            nbt.SetLong("TimesExisted", (long)TimesExisted);
            nbt.SetByte("IdWorld", IdWorld);
            nbt.SetInt("ChX", chPos.X);
            nbt.SetInt("ChY", chPos.Y);
            NBTTools.WriteToFile(nbt, pathName, true);
        }

        #endregion

        /// <summary>
        /// Добавить игрока в конкретный чанк для клиента
        /// </summary>
        public void AddChunkClient(int chunkPosX, int chunkPosY)
            => _clientChunks.Add(new ChunkPosition(chunkPosX, chunkPosY));

        /// <summary>
        /// Удалить игрока из конкретного чанка для клиента
        /// </summary>
        public void RemoveChunkClient(int chunkPosX, int chunkPosY)
        {
            _clientChunks.Remove(chunkPosX, chunkPosY);
            SendPacket(new PacketS21ChunkData(chunkPosX, chunkPosY));
        }

        #region Anchor

        /// <summary>
        /// Возвращает имеется ли хоть один чанк для загрузки
        /// </summary>
        public bool CheckLoadingChunks() => _loadingChunksSort.Count > 0;

        /// <summary>
        /// Вернуть координаты чанка для загрузки
        /// </summary>
        public ulong ReturnChunkForLoading()
        {
            ulong index = _loadingChunksSort.GetLast();
            _loadingChunksSort.RemoveLast();
            int x = Conv.IndexToChunkX(index);
            int y = Conv.IndexToChunkY(index);
            _loadingChunks.Remove(x, y);
            return index;
        }

        /// <summary>
        /// Добавить якорь в конкретный чанк
        /// </summary>
        /// <param name="chunkPosX">Позиция X чанка</param>
        /// <param name="chunkPosY">Позиция Y чанка</param>
        public void AddChunk(int chunkPosX, int chunkPosY)
            => _loadingChunks.Add(new ChunkPosition(chunkPosX, chunkPosY));

        /// <summary>
        /// Удалить якорь из конкретного чанка
        /// </summary>
        /// <param name="chunkPosX">Позиция X чанка</param>
        /// <param name="chunkPosY">Позиция Y чанка</param>
        public void RemoveChunk(int chunkPosX, int chunkPosY)
            => _loadingChunks.Remove(chunkPosX, chunkPosY);

        /// <summary>
        /// Установленный перемещенный якорь
        /// </summary>
        public void MountedMovedAnchor()
        {
            _UpOverviewChunkPrev();
            ChunkPosManagedX = ChunkPositionX;
            ChunkPosManagedY = ChunkPositionY;
            _FilterChunkLoadQueueRevers();
        }

        /// <summary>
        /// Изменение обзора,
        /// </summary>
        public bool IsChangeOverview() => OverviewChunk != OverviewChunkPrev;

        /// <summary>
        /// Необходимо ли смещение?
        /// </summary>
        public bool IsAnOffsetNecessary() 
            => ChunkPositionX != ChunkPosManagedX || ChunkPositionY != ChunkPosManagedY;

        #endregion

        /// <summary>
        /// Фильтрация очереди загрузки фрагментов от центра к краю (реверс)
        /// </summary>
        private void _FilterChunkLoadQueueRevers()
        {
            // Реверс спирали
            _loadingChunksSort.Clear();
            _clientChunksSort.Clear();
            int x, y, i, i2, i3;
            bool isClient = false;
            int overviewClient = OverviewChunk;
            int overview = FragmentManager.GetActiveRadiusAddServer(OverviewChunk, this);

            for (i = overview; i > 0; i--)
            {
                if (!isClient && i <= overviewClient)
                {
                    isClient = true;
                }
                i2 = ChunkPosManagedX - i;
                i3 = ChunkPosManagedX + i;
                y = i + ChunkPosManagedY;
                for (x = i2; x <= i3; x++)
                {
                    if (_loadingChunks.Contains(x, y)) _loadingChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                    if (isClient && _clientChunks.Contains(x, y)) _clientChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                }
                y = ChunkPosManagedY - i;
                for (x = i2; x <= i3; x++)
                {
                    if (_loadingChunks.Contains(x, y)) _loadingChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                    if (isClient && _clientChunks.Contains(x, y)) _clientChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                }
                i2 = ChunkPosManagedY - i + 1;
                i3 = ChunkPosManagedY + i - 1;
                x = i + ChunkPosManagedX;
                for (y = i2; y <= i3; y++)
                {
                    if (_loadingChunks.Contains(x, y)) _loadingChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                    if (isClient && _clientChunks.Contains(x, y)) _clientChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                }
                x = ChunkPosManagedX - i;
                for (y = i2; y <= i3; y++)
                {
                    if (_loadingChunks.Contains(x, y)) _loadingChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                    if (isClient && _clientChunks.Contains(x, y)) _clientChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                }
            }
            // Позиция где стоит игрок
            if (_loadingChunks.Contains(ChunkPosManagedX, ChunkPosManagedY))
            {
                _loadingChunksSort.Add(Conv.ChunkXyToIndex(ChunkPosManagedX, ChunkPosManagedY));
            }
            if (isClient && _clientChunks.Contains(ChunkPosManagedX, ChunkPosManagedY))
            {
                _clientChunksSort.Add(Conv.ChunkXyToIndex(ChunkPosManagedX, ChunkPosManagedY));
            }
        }

        /// <summary>
        /// Вызывается только на сервере у игроков для передачи перемещения
        /// </summary>
        private void _UpdatePlayer()
        {
            ulong index;
            int x, y;
            ChunkBase chunk;
            _loadedNull.Clear();
            byte quantityBatch = 0;
            while (_clientChunksSort.Count > 0 && quantityBatch < _desiredBatchSize)
            {
                index = _clientChunksSort.GetLast();
                _clientChunksSort.RemoveLast();
                x = Conv.IndexToChunkX(index);
                y = Conv.IndexToChunkY(index);

                chunk = GetWorld().GetChunk(x, y);
                // NULL по сути не должен быть!!!
                if (chunk != null && chunk.IsSendChunk)
                {
                    _clientChunks.Remove(x, y);
                    if (quantityBatch == 0)
                    {
                        // Отправляем перед партией закачки чанков
                        SendPacket(new PacketS20ChunkSend());
                    }
                    quantityBatch++;
                    SendPacket(new PacketS21ChunkData(chunk, 65535));
                }
                else
                {
                    _loadedNull.Add(index);
                }
            }
            if (quantityBatch > 0)
            {
                // Отправляем в конце партии закачки чанков
                SendPacket(new PacketS20ChunkSend(_clientChunksSort.Count == 0 
                    ? _desiredBatchSize : quantityBatch));
            }

            int count = _loadedNull.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    _clientChunksSort.Add(_loadedNull[i]);
                }
                // Запрос на пересортировку ClientChunks
            }
        }

        public override string ToString() => Login + " Lc:" 
            + _loadingChunks.ToString() + " Cc:" + _clientChunks.ToString() 
            + " dbs:" + _desiredBatchSize;
    }
}
