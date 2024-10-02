using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Vge.Games;
using Vge.NBT;
using Vge.Network;
using Vge.Network.Packets.Server;

namespace Vge.Management
{
    /// <summary>
    /// Объект игрока, как локального так и сетевого, не сущность
    /// </summary>
    public class PlayerServer
    {
        /// <summary>
        /// Уникальный порядковый номер игрока
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// Псевдоним игрока
        /// </summary>
        public readonly string Login;
        /// <summary>
        /// Хэш игрока по псевдониму
        /// </summary>
        public readonly string UUID;
        /// <summary>
        /// Хэш пароль игрока
        /// </summary>
        public string Token { get; private set; }
        /// <summary>
        /// Сетевой сокет
        /// </summary>
        public readonly SocketSide Socket;
        /// <summary>
        /// Флаг является ли этот игрок владельцем
        /// </summary>
        public readonly bool Owner;
        /// <summary>
        /// Пинг клиента в мс
        /// </summary>
        public int Ping { get; private set; } = -1;
        /// <summary>
        /// Указать причину удаления
        /// </summary>
        public string causeRemove = "";
        /// <summary>
        /// Сколько мили секунд эта сущность прожила
        /// </summary>
        public double TimesExisted { get; private set; } = 0;

        /// <summary>
        /// Последнее время пинга в милисекундах
        /// </summary>
        private long lastTimeServer;

        /// <summary>
        /// Основной сервер
        /// </summary>
        private readonly Server server;
        /// <summary>
        /// Имя пути игрока
        /// </summary>
        private readonly string pathName;

        /// <summary>
        /// Создать сетевого
        /// </summary>
        public PlayerServer(string login, string token, SocketSide socket, Server server)
        {
            Login = login;
            Token = GetHash(token);
            Socket = socket;
            this.server = server;
            UUID = GetHash(login);
            pathName = server.Settings.PathPlayers + UUID + ".dat";
            Owner = socket == null;
            Id = server.LastEntityId();
            lastTimeServer = server.Time();
        }

        /// <summary>
        /// Создать владельца
        /// </summary>
        public PlayerServer(string login, string token, Server server)
            : this(login, token, null, server) { }

        #region Ping

        /// <summary>
        /// Задать время пинга
        /// </summary>
        public void SetPing(long time)
        {
            lastTimeServer = server.Time();
            Ping = (Ping * 3 + (int)(lastTimeServer - time)) / 4;
        }

        /// <summary>
        /// Проверка времени игрока без пинга, если игрок не отвечал больше 30 секунд
        /// </summary>
        public bool TimeOut() => (server.Time() - lastTimeServer) > 30000;

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
            NBTTools.WriteToFile(nbt, pathName, true);
        }

        #endregion
    }
}
