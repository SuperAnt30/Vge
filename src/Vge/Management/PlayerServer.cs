using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Vge.Games;
using Vge.NBT;
using Vge.Network;
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
        /// Список чанков нужно проверить на загрузку или генерацию,
        /// должен формироваться по дистанции от игрока.
        /// </summary>
        public ListFast<ulong> LoadingChunks { get; private set; } = new ListFast<ulong>();

        /// <summary>
        /// Список чанков нужно проверить на загрузку или генерацию,
        /// должен формироваться по дистанции от игрока.
        /// </summary>
        private readonly MapChunk _loadingChunks = new MapChunk();

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
               // server.Filer.StartSection("Fragment" + OverviewChunk);
                server.Worlds.GetWorld(idWorld).Fragment.UpdateMountedMovingAnchor(this);
               // server.Filer.EndSectionLog();
                isPos = false;
            }
        }

        /// <summary>
        /// Отправить сетевой пакет этому игроку
        /// </summary>
        public void SendPacket(IPacket packet)
            => server.ResponsePacket(Socket, packet);

        /// <summary>
        /// Пройдены все проверки, отправляем нужные пакеты игроку
        /// </summary>
        public void JoinGame()
        {
            SendPacket(new PacketS03JoinGame(Id, UUID));
            SendPacket(new PacketS04TimeUpdate(server.TickCounter));
            SendPacket(new PacketS08PlayerPosLook(new System.Numerics.Vector3(chPos.X, chPos.Y, 0), 0, 0));
            // И другие пакеты, такие как позиция и инвентарь и прочее

            // Определяем в каком мире
            idWorld = 0;

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
        public WorldServer GetWorld() => server.Worlds.GetWorld(idWorld);

        /// <summary>
        /// Получить хэш по строке
        /// </summary>
        public static string GetHash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        public override string ToString() => Login;

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
            nbt.SetInt("ChX", chPos.X);
            nbt.SetInt("ChY", chPos.Y);
            NBTTools.WriteToFile(nbt, pathName, true);
        }

        #endregion

        #region Anchor

        /// <summary>
        /// Добавить якорь в конкретный чанк
        /// </summary>
        /// <param name="chunkPosX">Позиция X чанка</param>
        /// <param name="chunkPosY">Позиция Y чанка</param>
        public void AddChunk(int chunkPosX, int chunkPosY)
        {
            _loadingChunks.Add(new ChunkPosition(chunkPosX, chunkPosY));
            //if (isLoaded) LoadedChunks.Add(CurrentChunk);
            // LoadingChunks.Add(Conv.ChunkXyToIndex(chunkPosX, chunkPosY));
        }

        /// <summary>
        /// Удалить якорь из конкретного чанка
        /// </summary>
        /// <param name="chunkPosX">Позиция X чанка</param>
        /// <param name="chunkPosY">Позиция Y чанка</param>
        public void RemoveChunk(int chunkPosX, int chunkPosY)
        {
            _loadingChunks.Remove(chunkPosX, chunkPosY);
        }

        /// <summary>
        /// Фильтрация очереди загрузки фрагментов от центра к краю (реверс)
        /// </summary>
        private void _FilterChunkLoadQueueRevers()
        {
            // Реверс спирали
            LoadingChunks.Clear();
            int x, y, i, i2, i3;

            for (i = OverviewChunk; i > 0; i--)
            {
                i2 = ChunkPosManagedX - i;
                i3 = ChunkPosManagedX + i;
                y = i + ChunkPosManagedY;
                for (x = i2; x <= i3; x++)
                {
                    if (_loadingChunks.Contains(x, y)) LoadingChunks.Add(Conv.ChunkXyToIndex(x, y));
                }
                y = ChunkPosManagedY - i;
                for (x = i2; x <= i3; x++)
                {
                    if (_loadingChunks.Contains(x, y)) LoadingChunks.Add(Conv.ChunkXyToIndex(x, y));
                }
                i2 = ChunkPosManagedY - i + 1;
                i3 = ChunkPosManagedY + i - 1;
                x = i + ChunkPosManagedX;
                for (y = i2; y <= i3; y++)
                {
                    if (_loadingChunks.Contains(x, y)) LoadingChunks.Add(Conv.ChunkXyToIndex(x, y));
                }
                x = ChunkPosManagedX - i;
                for (y = i2; y <= i3; y++)
                {
                    if (_loadingChunks.Contains(x, y)) LoadingChunks.Add(Conv.ChunkXyToIndex(x, y));
                }
            }
            // Позиция где стоит игрок
            if (_loadingChunks.Contains(ChunkPosManagedX, ChunkPosManagedY))
            {
                LoadingChunks.Add(Conv.ChunkXyToIndex(ChunkPosManagedX, ChunkPosManagedY));
            }
        }

        /*
        private static readonly int[][] xzDirectionsConst
            = new int[][] { new int[] { 1, 0 }, new int[] { 0, 1 }, new int[] { -1, 0 }, new int[] { 0, -1 } };

        /// <summary>
        /// Фильтрация очереди загрузки фрагментов от центра к краю
        /// </summary>
        private void _FilterChunkLoadQueue()
        {
            // Алгоритм метода взят с minecraft 1.8

            LoadingChunks.Clear();
            int counter = 0;
            int chx = ChunkPosManagedX;
            int chy = ChunkPosManagedY;
            int x = 0;
            int y = 0;
            int overview = OverviewChunk;
            int i, i2, i3;
            int[] vec;
            ulong index;

            // Позиция где стоит игрок
            index = Conv.ChunkXyToIndex(chx, chy);
            if (_loadingChunks.Contains(index)) LoadingChunks.Add(index);
            // Пропегаемся по спирали квадратной
            for (i = 1; i <= overview * 2; i++)
            {
                for (i2 = 0; i2 < 2; ++i2)
                {
                    vec = xzDirectionsConst[counter++ % 4];
                    for (i3 = 0; i3 < i; i3++)
                    {
                        x += vec[0];
                        y += vec[1];
                        index = Conv.ChunkXyToIndex(x + chx, y + chy);
                        if (_loadingChunks.Contains(index)) LoadingChunks.Add(index);
                    }
                }
            }

            // Добиваем последнюю сторону
            vec = xzDirectionsConst[counter++ % 4];
            for (i = 0; i < overview * 2; i++)
            {
                x += vec[0];
                y += vec[1];
                index = Conv.ChunkXyToIndex(x + chx, y + chy);
                if (_loadingChunks.Contains(index)) LoadingChunks.Add(index);
            }
        }*/

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
        public bool IsAnOffsetNecessary() => ChunkPositionX != ChunkPosManagedX || ChunkPositionY != ChunkPosManagedY;

        #endregion

    }
}
