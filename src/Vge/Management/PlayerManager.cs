using System.Collections.Generic;
using Vge.Games;
using Vge.Network;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;

namespace Vge.Management
{
    /// <summary>
    /// Объект управления пользователями на сервере
    /// </summary>
    public class PlayerManager
    {
        /// <summary>
        /// Основной сервер
        /// </summary>
        private readonly Server server;

        /// <summary>
        /// Колекция сетевых игроков
        /// </summary>
        private List<PlayerServer> players = new List<PlayerServer>();
        /// <summary>
        /// объект игрока владельца
        /// </summary>
        private PlayerServer playerOwner;

        public PlayerManager(Server server)
        {
            this.server = server;
        }

        #region AddRemoveCount

        /// <summary>
        /// Количество сетевый игроков
        /// </summary>
        public int PlayerNetCount() => players.Count;

        /// <summary>
        /// Добавить игрока владельца
        /// </summary>
        public void PlayerOwnerAdd(string login, string token)
        {
            playerOwner = new PlayerServer(login, token, server);
            // Проверка токена не важна для владельца
            GetPlayerData(playerOwner);
        }

        public bool PlayerRemove(SocketSide socketSide)
        {
            foreach(PlayerServer player in players)
            {
                if (player.Socket.Equals(socketSide))
                {
                    players.Remove(player);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Найти объект Игрока по сокету, если нет вернёт null
        /// </summary>
        private PlayerServer FindPlayerBySocket(SocketSide socketSide)
        {
            foreach (PlayerServer player in players)
            {
                if (player.Socket.Equals(socketSide))
                {
                    return player;
                }
            }
            return null;
        }

        #endregion

        #region All

        /// <summary>
        /// Всех сетевый игроков выкинуть с пречиной
        /// </summary>
        public void AllNetPlayersDisconnect(string cause)
        {
            if (players.Count > 0)
            {
                for (int i = players.Count - 1; i >= 0; i--)
                {
                    server.PlayerDisconnect(players[i].Socket, cause);
                }
            }
        }

        /// <summary>
        /// Отправить пакет всем игрокам
        /// </summary>
        public void SendToAll(IPacket packet)
        {
            server.ResponsePacketOwner(packet);
            foreach (PlayerServer player in players)
            {
                player.SendPacket(packet);
            }
        }

        #endregion

        #region SentPacket

        /// <summary>
        /// Пакет проверки сетевого логина игрока
        /// </summary>
        public void LoginStart(SocketSide socketSide, PacketC02LoginStart packet)
        {
            if (packet.GetVersion() != Ce.IndexVersion)
            {
                // Клиент другой версии
                server.Log.Server(Srl.ServerVersionAnother, packet.GetLogin(), packet.GetVersion());
                socketSide.SendPacket(new PacketS02LoadingGame(PacketS02LoadingGame.EnumStatus.VersionAnother));
                return;
            }
            string login = packet.GetLogin();

            // Проверяем корректность никнейма
            if (login == "")
            {
                socketSide.SendPacket(new PacketS02LoadingGame(PacketS02LoadingGame.EnumStatus.LoginIncorrect));
                return;
            }

            // Проверяем имеется ли такой игрок с таким же именем по uuid в сети
            string uuid = PlayerServer.GetHash(login);
            foreach (PlayerServer player in players)
            {
                if (uuid.Equals(player.UUID))
                {
                    socketSide.SendPacket(new PacketS02LoadingGame(PacketS02LoadingGame.EnumStatus.LoginDuplicate));
                    return;
                }
            }

            // Создаём объект Игрока
            PlayerServer playerServer = new PlayerServer(login, packet.GetToken(), socketSide, server);

            // Загружаем данные игрока если имеются
            if (!GetPlayerData(playerServer))
            {
                // Проверка токена не прошла
                socketSide.SendPacket(new PacketS02LoadingGame(PacketS02LoadingGame.EnumStatus.InvalidToken));
                return;
            }

            // Все проверки прошли, создаём Игрока и даём ответ
            players.Add(playerServer);
            socketSide.SendPacket(new PacketS03JoinGame(playerServer.Id, playerServer.UUID));
        }

        /// <summary>
        /// Соединение игрока владельца
        /// </summary>
        public void JoinGameOwner()
            => playerOwner.SendPacket(new PacketS03JoinGame(playerOwner.Id, playerOwner.UUID));

        #endregion

        /// <summary>
        /// TODO::TestUserAllKill
        /// </summary>
        public void TestUserAllKill()
            => AllNetPlayersDisconnect(Sr.ThrownOut);


        #region WriteReadSpawn

        /// <summary>
        /// Получить данные игрока, возращает false если токены разные
        /// </summary>
        private bool GetPlayerData(PlayerServer entityPlayer)
        {
            string token = entityPlayer.Token;
            if (!entityPlayer.ReadFromFile())
            {
                // Игрок впервые зашёл, создаём
                CreatePlayer(entityPlayer);
                // И тут же записываем данные
                entityPlayer.WriteFromFile();
                return true;
            }
            // Проверка токенов
            return token.Equals(entityPlayer.Token);
        }

        /// <summary>
        /// Прочесть данные игрока с файла, возращает true если файл существола
        /// </summary>
        private bool ReadPlayerFromFile(PlayerServer entityPlayer)
        {
            return false;
        }

        /// <summary>
        /// Создать игрока, тут первый спавн игрока
        /// </summary>
        private void CreatePlayer(PlayerServer entityPlayer)
        {

        }

        #endregion
    }
}
