using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Vge.Games;
using Vge.NBT;
using Vge.Network;
using Vge.Network.Packets.Server;
using Vge.Util;
using Vge.World;
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
        ///  В какой позиции X чанка было обработка видимых чанков
        /// </summary>
        public int ChunkPosManagedX { get; private set; }
        /// <summary>
        ///  В какой позиции Y чанка было обработка видимых чанков
        /// </summary>
        public int ChunkPosManagedY { get; private set; }

        /// <summary>
        /// Список чанков нужно проверить на загрузку или генерацию,
        /// должен формироваться по дистанции от игрока.
        /// </summary>
        public ListFast<ulong> LoadingChunks { get; private set; } = new ListFast<ulong>();

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

            // Обновляем чанк обработки
            UpChunkPosManaged();
            // Обновляем обзор прошлого такта
            UpOverviewChunkPrev();

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
            //if (isLoaded) LoadedChunks.Add(CurrentChunk);
           // LoadingChunks.Add(Conv.ChunkXyToIndex(chunkPosX, chunkPosY));
        }

        /// <summary>
        /// Обновить обзор прошлого такта
        /// </summary>
        public override void UpOverviewChunkPrev()
        {
            base.UpOverviewChunkPrev();
            //int overviewChunk = OverviewChunk;
            //if (overviewChunk < PlayerManager.minRadius) overviewChunk = PlayerManager.minRadius;
            //DistSqrt = MvkStatic.DistSqrtTwo2d[overviewChunk + PlayerManager.addServer];
        }

        /// <summary>
        /// Задать чанк обработки
        /// </summary>
        public void UpChunkPosManaged()
        {
            ChunkPosManagedX = ChunkPositionX;
            ChunkPosManagedY = ChunkPositionY;
        }

        #endregion

    }
}
