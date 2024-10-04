using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Vge.Games;
using Vge.NBT;
using Vge.Network;
using Vge.Network.Packets.Server;
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
        public double TimesExisted { get; private set; } = 0;

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

        #region Ping

        /// <summary>
        /// Добавить время к игроку
        /// </summary>
        public void AddDeltaTime() => TimesExisted += server.DeltaTime;

        #endregion

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
        }

        /// <summary>
        /// Игрок покидает игру
        /// </summary>
        public void LeftGame()
        {
            // Сохраняем
            WriteToFile();
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
    }
}
