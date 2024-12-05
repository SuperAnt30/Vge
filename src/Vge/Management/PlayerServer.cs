using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Vge.Games;
using Vge.NBT;
using Vge.Network;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
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

        #region Anchor (якорь)

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
        public int ChunkPositionX => Position.ChunkPositionX;
        /// <summary>
        /// Координата Y в каком чанке находится
        /// </summary>
        public int ChunkPositionY => Position.ChunkPositionZ;

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
        /// Рфзмер партии закачки чанков
        /// </summary>
        private byte _batchSizeReceive;
        /// <summary>
        /// Размер партии распаковки чанков
        /// </summary>
        private byte _batchSizeUnpack;
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
            _batchSizeReceive = _batchSizeUnpack 
                = _desiredBatchSize = Ce.MinDesiredBatchSize;
            Id = server.LastEntityId();
            _lastTimeServer = server.Time();
        }

        /// <summary>
        /// Создать владельца
        /// </summary>
        public PlayerServer(string login, string token, GameServer server)
            : this(login, token, null, server) { }

        #region Update

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void Update()
        {
            // Добавить время к игроку
            TimesExisted += server.DeltaTime;

            // Тут надо анализ сделать было ли перемещение
            if (IsPositionChange() || IsChangeOverview())
            {
                //server.Filer.StartSection("Fragment" + OverviewChunk);
                GetWorld().Fragment.UpdateMountedMovingAnchor(this);
                //server.Filer.EndSectionLog();
                PositionPrev.Set(Position);
            }

            _UpdatePlayer();
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

                    SendPacket(new PacketS21ChunkData(chunk, true, uint.MaxValue));
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

        #endregion

        #region Packet

        /// <summary>
        /// Отправить сетевой пакет этому игроку
        /// </summary>
        public void SendPacket(IPacket packet)
            => server.ResponsePacket(Socket, packet);

        /// <summary>
        /// Пакет: позиции игрока
        /// </summary>
        public void PacketPlayerPosition(PacketC04PlayerPosition packet)
        {
            Position.Set(packet.Position);

            // TODO::2024-11-29 временная смена мира
            if (IdWorld != packet.World)
            {
                // Смена мира
                ChangeWorld(packet.World);
            }
        }

        /// <summary>
        /// Пакет: Игрок копает / ломает
        /// </summary>
        public void PacketPlayerDigging(PacketC07PlayerDigging packet)
        {
            // Временно!
            // Уничтожение блока
            WorldServer world = GetWorld();
            BlockState blockState = world.GetBlockState(packet.GetBlockPos());
            BlockBase block = blockState.GetBlock();

            world.SetBlockToAir(packet.GetBlockPos(), world.IsRemote ? 14 : 31);
            //pause = entityPlayer.PauseTimeBetweenBlockDestruction();
        }

        /// <summary>
        /// Пакет: Установки или взаимодействия с блоком
        /// </summary>
        public void PacketPlayerBlockPlacement(PacketC08PlayerBlockPlacement packet)
        {
            // Временно устанваливаем блок
            ushort idBlock = 0;
            for (ushort i = 0; i < Ce.Blocks.BlockAlias.Length; i++)
            {
                if (Ce.Blocks.BlockAlias[i] == "Brol")
                {
                    idBlock = i;
                    break;
                }
            }

            WorldServer world = GetWorld();
            BlockState blockState = world.GetBlockState(packet.GetBlockPos());
            BlockBase block = blockState.GetBlock();
            BlockPos blockPos = packet.GetBlockPos().Offset(packet.Side);
            world.SetBlockState(blockPos, new BlockState(idBlock), world.IsRemote ? 14 : 31);
        }

        /// <summary>
        /// Пакет: Параметры игрока
        /// </summary>
        public void PacketPlayerSetting(PacketC15PlayerSetting packet)
            => SetOverviewChunk(packet.OverviewChunk);
        
        /// <summary>
        /// Пакет: Подтверждение фрагментов
        /// </summary>
        public void PacketAcknowledgeChunks(PacketC20AcknowledgeChunks packet)
        {
            //" dbs:" + _desiredBatchSize;
            if (packet.IsLoad)
            {
                // Желаемое количество при передаче по сети
                _batchSizeReceive = Sundry.RecommendedQuantityBatch(packet.Time,
                packet.Quantity, _batchSizeReceive, Ce.MaxDesiredBatchSize, Ce.MaxBatchChunksTimeUnpack);
            }
            else
            {
                // Желаемое количество при распаковке на клиенте
                _batchSizeUnpack = Sundry.RecommendedQuantityBatch(packet.Time,
                packet.Quantity, _batchSizeUnpack, Ce.MaxDesiredBatchSize, Ce.MaxBatchChunksTimeUnpack);
            }
            // Минимальное используем
            _desiredBatchSize = Mth.Min(_batchSizeReceive, _batchSizeUnpack);
        }

        #endregion

        #region JoinLeftGame

        /// <summary>
        /// Пройдены все проверки, отправляем нужные пакеты игроку
        /// </summary>
        public void JoinGame()
        {
            // Id игрока
            SendPacket(new PacketS03JoinGame(Id, UUID));
            // Таблицу блоков если не владелец
            if (!Owner)
            {
                SendPacket(new PacketS05TableBlocks(Ce.Blocks.BlockAlias));
            }
            // Тикущий счётчик тика сервера
            SendPacket(new PacketS04TimeUpdate(server.TickCounter));
            // Информацию о мире в каком игрок находиться
            SendPacket(new PacketS07RespawnInWorld(IdWorld, GetWorld().Settings));
            // Местоположение игрока
            SendPacket(new PacketS08PlayerPosLook(Position));
            // И другие пакеты, такие как позиция и инвентарь и прочее


            // Установленный перемещенный якорь
            MountedMovedAnchor();

            // Вносим в менеджер фрагментов игрока
            GetWorld().Fragment.AddAnchor(this);
            
            //
            //isPos = true;
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

        #endregion

        #region World

        /// <summary>
        /// Получить мир в котором находится игрок
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        #endregion

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
                    PositionPrev.X = Position.X = nbt.GetFloat("PosX");
                    PositionPrev.Y = Position.Y = nbt.GetFloat("PosY");
                    PositionPrev.Z = Position.Z = nbt.GetFloat("PosZ");
                    PositionPrev.Yaw = Position.Yaw = nbt.GetFloat("Yaw");
                    PositionPrev.Pitch = Position.Pitch = nbt.GetFloat("Pitch");
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
            nbt.SetFloat("PosX", Position.X);
            nbt.SetFloat("PosY", Position.Y);
            nbt.SetFloat("PosZ", Position.Z);
            nbt.SetFloat("Yaw", Position.Yaw);
            nbt.SetFloat("Pitch", Position.Pitch);
            NBTTools.WriteToFile(nbt, pathName, true);
        }

        #endregion

        #region ChunkAddRemove

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

        #endregion

        #region Anchor

        /// <summary>
        /// Проверить имеется ли чанк в загрузке (loadingChunksSort)
        /// </summary>
        public bool IsLoadingChunks(int x, int y)
            => _loadingChunksSort.Contains(Conv.ChunkXyToIndex(x, y));

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

        #endregion

        /// <summary>
        /// Получить время в милисекундах с сервера
        /// </summary>
        protected override long _Time() => server.Time();

        /// <summary>
        /// Получить хэш по строке
        /// </summary>
        public static string GetHash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        public override string ToString() => Login + " Lc:" 
            + _loadingChunks.ToString() + " Cc:" + _clientChunks.ToString() 
            + " dbs:" + _desiredBatchSize;
    }
}
