using System.Collections.Generic;
using Vge.Games;
using Vge.Network;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;

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
        private readonly List<PlayerServer> players = new List<PlayerServer>();
        /// <summary>
        /// объект игрока владельца
        /// </summary>
        private PlayerServer playerOwner;
        /// <summary>
        /// Список игроков которые надо запустить в ближайшем такте
        /// </summary>
        private readonly DoubleList<PlayerServer> playerStartList = new DoubleList<PlayerServer>();
        /// <summary>
        /// Список сетевых игроков котрые надо выгрузить в ближайшем такте
        /// </summary>
        private readonly DoubleList<int> playerRemoveList = new DoubleList<int>();
        /// <summary>
        /// Массив кэша удалённых игроков в тике для защиты перезахода
        /// </summary>
        private readonly ListFast<int> cachePlayerRemoveList = new ListFast<int>(10);

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

        /// <summary>
        /// Поставить в очередь на удаление игрока с сервера и указать причину
        /// </summary>
        public void PlayerRemove(SocketSide socketSide, string cause)
            => PlayerRemove(FindPlayerBySocket(socketSide), cause);

        /// <summary>
        /// Поставить в очередь на удаление игрока с сервера и указать причину
        /// </summary>
        public void PlayerRemove(PlayerServer playerServer, string cause)
        {
            if (playerServer != null)
            {
                playerServer.causeRemove = cause;
                playerRemoveList.Add(playerServer.Id);
                if (playerServer.Owner)
                {
                    PlayerLeftGame(playerServer);
                    server.Stop();
                }
            }
        }

        /// <summary>
        /// Найти объект Игрока по сокету, если нет вернёт null
        /// </summary>
        public PlayerServer FindPlayerBySocket(SocketSide socketSide)
        {
            if (socketSide == null)
            {
                // Если нет сокета, то это владелец
                return playerOwner;
            }
            foreach (PlayerServer player in players)
            {
                if (player.Socket.Equals(socketSide))
                {
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// Найти объект Игрока по id, если нет вернёт null
        /// </summary>
        public PlayerServer FindPlayerById(int id)
        {
            if (playerOwner.Id == id)
            {
                // Это владелец
                return playerOwner;
            }
            // Проверка сетевых
            foreach (PlayerServer player in players)
            {
                if (player.Id == id)
                {
                    return player;
                }
            }
            return null;
        }

        #endregion

        #region All

        /// <summary>
        /// Удалить всех игроков при остановки сервера
        /// </summary>
        public void PlayersRemoveStopingServer()
        {
            if (players.Count > 0)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    PlayerRemove(players[i], Sr.StopServer);
                }
            }
            PlayerRemove(playerOwner, Sr.StopServer);
            UpdateRemovingPlayers();
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

            // Поставить в очередь на cоединение сетевого игрока
            playerStartList.Add(playerServer);
        }

        /// <summary>
        /// Поставить в очередь на cоединение игрока владельца
        /// </summary>
        public void JoinGameOwner() => playerStartList.Add(playerOwner);

        #endregion

        #region Updates

        /// <summary>
        /// Тик на сервере
        /// </summary>
        public void Update()
        {
            // Удаляем игроков
            UpdateRemovingPlayers();

            // Добавляем игроков
            if (!playerStartList.Empty())
            {
                playerStartList.Step();
                int count = playerStartList.CountBackward;
                for (int i = 0; i < count; i++)
                {
                    PlayerJoinGame(playerStartList.GetNext());
                }
                cachePlayerRemoveList.Clear();
            }

            // Обновление игроков
            PlayerServerUpdate(playerOwner);
            for (int i = 0; i < players.Count; i++)
            {
                PlayerServerUpdate(players[i]);
            }

        }

        /// <summary>
        /// Обновление раз в тик
        /// </summary>
        private void PlayerServerUpdate(PlayerServer entityPlayer)
        {
            if (!entityPlayer.Owner && entityPlayer.TimeOut())
            {
                // На сервере пометка убрать
                PlayerRemove(entityPlayer, Sr.TimeOut);
            }
        }

        /// <summary>
        ///  Удаляем игроков в тике
        /// </summary>
        private void UpdateRemovingPlayers()
        {
            if (!playerRemoveList.Empty())
            {
                playerRemoveList.Step();
                int id;
                int count = playerRemoveList.CountBackward;
                cachePlayerRemoveList.Clear();
                for (int i = 0; i < count; i++)
                {
                    id = playerRemoveList.GetNext();
                    cachePlayerRemoveList.Add(id);
                    PlayerLeftGame(FindPlayerById(id));
                }
            }
        }

        /// <summary>
        /// Подключаем игрока в игру, прошли все проверки на игрока
        /// </summary>
        private void PlayerJoinGame(PlayerServer player)
        {
            if (player != null && !cachePlayerRemoveList.Contains(player.Id))
            {
                if (!player.Owner)
                {
                    // Если это не владелец то добавляем в массив сетевых игроков
                    players.Add(player);
                }
                player.JoinGame();
            }
        }

        /// <summary>
        /// Вышел игрок с игры
        /// </summary>
        private void PlayerLeftGame(PlayerServer player)
        {
            if (player != null)
            {
                if (!player.Owner)
                {
                    // Если это не владелец то добавляем в массив сетевых игроков
                    players.Remove(player);
                    if (player.Socket.IsConnect())
                    {
                        // Сокет не закрыт, значит закрытие на стороне сервера, надо отправить сообщение
                        // TODO::2024-10-01 тут можно отправить строковое сообщение игроку до его разрыва связи с его причиной
                        server.PlayerDisconnect(player.Socket, player.causeRemove);
                    }
                }
                player.LeftGame();
            }
        }

        #endregion

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
