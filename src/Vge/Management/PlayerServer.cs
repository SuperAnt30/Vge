//#define PhysicsServer
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Vge.Entity;
using Vge.Entity.List;
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
        public string CauseRemove = "";
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
        /// В какой позиции X чанка было обработка видимых чанков
        /// </summary>
        public int ChunkPosManagedX { get; private set; }
        /// <summary>
        /// В какой позиции Y чанка было обработка видимых чанков
        /// </summary>
        public int ChunkPosManagedZ { get; private set; }

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
        private readonly GameServer _server;
        /// <summary>
        /// Имя пути к папке игрока
        /// </summary>
        private readonly string _pathName;
        /// <summary>
        /// Смена обзора чанков, для трекера
        /// </summary>
        private bool _flagOverviewChunkChanged;
        /// <summary>
        /// Список сущностей не игрок, которые в ближайшем тике будут удалены
        /// </summary>
        private ListMessy<int> _destroyedItemsNetCache = new ListMessy<int>();

        /// <summary>
        /// Создать сетевого
        /// </summary>
        public PlayerServer(string login, string token, SocketSide socket, GameServer server)
        {
            Login = login;
            Token = GetHash(token);
            _InitIndexPlayer();
            Socket = socket;
            _server = server;
            UUID = GetHash(login);
            _pathName = server.Settings.PathPlayers + UUID + ".dat";
            Owner = socket == null;
            _batchSizeReceive = _batchSizeUnpack 
                = _desiredBatchSize = Ce.MinDesiredBatchSize;
            Id = server.LastEntityId();
            Eye = Height * .85f;
            _lastTimeServer = server.Time();
            Render = new EntityRenderBase(this);
#if PhysicsServer
            Physics = new PhysicsPlayer(GetWorld().Collision, this);
            Physics.SetImpulse(.8f);
#endif
        }

        /// <summary>
        /// Создать владельца
        /// </summary>
        public PlayerServer(string login, string token, GameServer server)
            : this(login, token, null, server) { }

#region Tracker

        /// <summary>
        /// Была ли смена обзора чанков, для трекера
        /// </summary>
        public override bool IsOverviewChunkChanged() => _flagOverviewChunkChanged;

        /// <summary>
        /// Сделана смена обзора чанков, для трекера
        /// </summary>
        public override void MadeOverviewChunkChanged() => _flagOverviewChunkChanged = false;

#endregion

#region GetSet

        /// <summary>
        /// Изменить позицию игрока на стороне сервера
        /// </summary>
        public void SetPositionServer(float x, float y, float z)
        {
            PosX = x;
            PosY = y;
            PosZ = z;
            // Местоположение игрока
            SendPacket(new PacketS08PlayerPosLook(PosX, PosY, PosZ, RotationYaw, RotationPitch));
        }

#endregion

#region Update

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void Update()
        {
            // Добавить время к игроку
            TimesExisted += _server.DeltaTime;

#if PhysicsServer
            // Расчитать перемещение в объекте физика
            Physics.LivingUpdate();
           // if (Physics.IsMotionChange)
            {
                // Только перемещение
                SendPacket(new PacketS08PlayerPosLook(PosX, PosY, PosZ));
                LevelMotionChange = 1;
            }
#endif

            // Тут надо анализ сделать было ли перемещение
            if (IsPositionChange() || IsChangeOverview())
            {
                //server.Filer.StartSection("Fragment" + OverviewChunk);
                GetWorld().Fragment.UpdateMountedMovingAnchor(this);
                //server.Filer.EndSection();
                PosPrevX = PosX;
                PosPrevY = PosY;
                PosPrevZ = PosZ;
            }
            if (IsRotationChange())
            {
                RotationPrevYaw = RotationYaw;
                RotationPrevPitch = RotationPitch;
            }

            // Отправляем запрос на удаление сущностей которые не видим
            if (_destroyedItemsNetCache.Count > 0)
            {
                SendPacket(new PacketS13DestroyEntities(_destroyedItemsNetCache.ToArray()));
                _destroyedItemsNetCache.Clear();
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
                    // TODO::2024-12-07 подумать как (PacketS21ChunkData) создание вынести в поток мира!
                    // В основном потоке загрузка компрессия чанков, занимает примерн 1 мс за чанк
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendPacket(IPacket packet)
            => _server.ResponsePacket(Socket, packet);

        /// <summary>
        /// Отправить системное сообщение конкретному игроку
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendMessage(string message) 
            => SendPacket(new PacketS3AMessage(message));

        /// <summary>
        /// Сущность которую надо удалить у клиента
        /// </summary>
        public void SendRemoveEntity(EntityBase entity)
        {
            if (entity is PlayerServer)
            {
                SendPacket(new PacketS13DestroyEntities(new int[] { entity.Id }));
            }
            else
            {
                _destroyedItemsNetCache.Add(entity.Id);
            }
        }

        /// <summary>
        /// Пакет: позиции игрока
        /// </summary>
        public void PacketPlayerPosition(PacketC04PlayerPosition packet)
        {
            if (packet.IsPosition)
            {
                PosX = packet.X;
                PosY = packet.Y;
                PosZ = packet.Z;
            }
            if (packet.IsRotate)
            {
                RotationYaw = packet.Yaw;
                RotationPitch = packet.Pitch;
            }
            LevelMotionChange = 1;
        }


        private bool isBox = true;

        /// <summary>
        /// Пакет: Игрок копает / ломает
        /// </summary>
        public void PacketPlayerDigging(PacketC07PlayerDigging packet)
        {
            // Временно!
            // Уничтожение блока
            WorldServer world = GetWorld();
            BlockState blockState = world.GetBlockState(packet.GetBlockPos());
            if (packet.Digging == PacketC07PlayerDigging.EnumDigging.Destroy)
            {
                BlockBase block = blockState.GetBlock();
                world.SetBlockToAir(packet.GetBlockPos(), world.IsRemote ? 14 : 31);
                //pause = entityPlayer.PauseTimeBetweenBlockDestruction();
            }
            else
            {
                // TODO::2025-02-10 Временно спавн моба
                for (int i = 0; i < 1000; i++)
                {
                    int id = _server.Worlds.GetDebugIndex(isBox);
                    if (id == -1)
                    {
                        return;
                    }
                    try
                    {
                        isBox = !isBox;
                        //TODO::2025-06-04 Спавн сущности на сервере, продумать удобным!!!
                        // Для сервера
                        EntityThrowable entityThrowable = Ce.ModelEntities.CreateEntityServer((ushort)id, world.Collision) as EntityThrowable;
                        entityThrowable.InitRun(this, i);
                        entityThrowable.SetEntityId(_server.LastEntityId());
                        world.SpawnEntityInWorld(entityThrowable);
                    }
                    catch (Exception ex)
                    {
                        return;
                    }
                }
            }
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
                if (Ce.Blocks.BlockAlias[i] == "Debug") //Debug GlassBlue
                {
                    idBlock = i;
                    break;
                }
            }

            // Определяем на какую сторону смотрит игрок
            Pole pole = PoleConvert.FromAngle(RotationYaw);

            WorldServer world = GetWorld();
            BlockState blockState = new BlockState(idBlock);// world.GetBlockState(packet.GetBlockPos());
            BlockBase block = blockState.GetBlock();

            BlockPos blockPos = packet.GetBlockPos().Offset(packet.Side);
            // TODO::ВРЕМЕННО!!!
            blockState = block.OnBlockPlaced(world, packet.GetBlockPos(), blockState, pole, packet.Facing);
            //block.OnBlockPlaced(world, packet.GetBlockPos(), blockState, packet.Side, packet.Facing);
            world.SetBlockState(blockPos, blockState, world.IsRemote ? 14 : 31);
        }

        /// <summary>
        /// Пакет: Параметры игрока
        /// </summary>
        public void PacketPlayerSetting(PacketC15PlayerSetting packet)
        {
            SetOverviewChunk(packet.OverviewChunk);
            // Изменён обзор корректировка трекеров сущностей
            _flagOverviewChunkChanged = true;
        }
        
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
                //server.Log.Log("bsr:" + _batchSizeReceive + " T:" + packet.Time);
            }
            else
            {
                // Желаемое количество при распаковке на клиенте
                _batchSizeUnpack = Sundry.RecommendedQuantityBatch(packet.Time,
                packet.Quantity, _batchSizeUnpack, Ce.MaxDesiredBatchSize, Ce.MaxBatchChunksTimeUnpack);
                //server.Log.Log("bsu:" + _batchSizeUnpack + " T:" + packet.Time);
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
            // Тикущий счётчик тика сервера
            SendPacket(new PacketS04TimeUpdate(_server.TickCounter));
            // Информацию о мире в каком игрок находиться
            SendPacket(new PacketS07RespawnInWorld(IdWorld, GetWorld().Settings));

            // Местоположение игрока
            SendPacket(new PacketS08PlayerPosLook(PosX, PosY, PosZ, RotationYaw, RotationPitch));
            // И другие пакеты, такие как позиция и инвентарь и прочее

            
            // Вносим в менеджер фрагментов игрока
            GetWorld().Fragment.AddAnchor(this);

            // Установленный перемещенный якорь
            MountedMovedAnchor();

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
        public WorldServer GetWorld() => _server.Worlds.GetWorld(IdWorld);

        /// <summary>
        /// Смена мира, передаём новый id мира
        /// </summary>
        public void ChangeWorld(byte newIdWorld)
        {
            GetWorld().RemovePlayerInWorldForNextWorld(this);
            // Смена id мира
            IdWorld = newIdWorld;
            SendPacket(new PacketS07RespawnInWorld(IdWorld, GetWorld().Settings));
            // Вносим в менеджер фрагментов игрока
            IsDead = false;
            GetWorld().PlayerForNextWorld(this);
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
            if (File.Exists(_pathName))
            {
                try
                {
                    TagCompound nbt = NBTTools.ReadFromFile(_pathName, true);
                    Token = nbt.GetString("Token");
                    TimesExisted = nbt.GetLong("TimesExisted");
                    IdWorld = nbt.GetByte("IdWorld");
                    PosPrevX = PosX = nbt.GetFloat("PosX");
                    PosPrevY = PosY = nbt.GetFloat("PosY");
                    PosPrevZ = PosZ = nbt.GetFloat("PosZ");
                    RotationPrevYaw = RotationYaw = nbt.GetFloat("Yaw");
                    RotationPrevPitch = RotationPitch = nbt.GetFloat("Pitch");
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
            GameFile.CheckPath(_server.Settings.PathPlayers);
            TagCompound nbt = new TagCompound();
            nbt.SetString("Token", Token);
            nbt.SetLong("TimesExisted", (long)TimesExisted);
            nbt.SetByte("IdWorld", IdWorld);
            nbt.SetFloat("PosX", PosX);
            nbt.SetFloat("PosY", PosY);
            nbt.SetFloat("PosZ", PosZ);
            nbt.SetFloat("Yaw", RotationYaw);
            nbt.SetFloat("Pitch", RotationPitch);
            NBTTools.WriteToFile(nbt, _pathName, true);
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
            ChunkPosManagedZ = ChunkPositionZ;
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
            => ChunkPositionX != ChunkPosManagedX || ChunkPositionZ != ChunkPosManagedZ;

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
                y = i + ChunkPosManagedZ;
                for (x = i2; x <= i3; x++)
                {
                    if (_loadingChunks.Contains(x, y)) _loadingChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                    if (isClient && _clientChunks.Contains(x, y)) _clientChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                }
                y = ChunkPosManagedZ - i;
                for (x = i2; x <= i3; x++)
                {
                    if (_loadingChunks.Contains(x, y)) _loadingChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                    if (isClient && _clientChunks.Contains(x, y)) _clientChunksSort.Add(Conv.ChunkXyToIndex(x, y));
                }
                i2 = ChunkPosManagedZ - i + 1;
                i3 = ChunkPosManagedZ + i - 1;
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
            if (_loadingChunks.Contains(ChunkPosManagedX, ChunkPosManagedZ))
            {
                _loadingChunksSort.Add(Conv.ChunkXyToIndex(ChunkPosManagedX, ChunkPosManagedZ));
            }
            if (isClient && _clientChunks.Contains(ChunkPosManagedX, ChunkPosManagedZ))
            {
                _clientChunksSort.Add(Conv.ChunkXyToIndex(ChunkPosManagedX, ChunkPosManagedZ));
            }
        }

        #endregion

        #region Input физика на сервере
#if PhysicsServer

        public void PacketInput(PacketC0CInput packet)
        {
            Physics.Movement.Forward = packet.Forward;
            Physics.Movement.Back = packet.Back;
            Physics.Movement.Left = packet.Left;
            Physics.Movement.Right = packet.Right;
            Physics.Movement.Jump = packet.Jump;
            Physics.Movement.Sneak = packet.Sneak;
            Physics.Movement.Sprinting = packet.Sprinting;
        }

        public void PacketInputRotate(PacketC0DInputRotate packet)
        {
            RotationYaw = packet.Yaw;
            RotationPitch = packet.Pitch;
            LevelMotionChange = 1;
        }

#endif
        #endregion

        /// <summary>
        /// Задать импульс
        /// </summary>
        public override void SetPhysicsImpulse(float x, float y, float z)
            => SendPacket(new PacketS08PlayerPosLook(x, y, z));

        /// <summary>
        /// Получить время в милисекундах с сервера
        /// </summary>
        protected override long _Time() => _server.Time();

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
