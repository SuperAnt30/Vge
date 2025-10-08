﻿using System.Collections.Generic;
using Vge.Command;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Network;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Realms;
using Vge.TileEntity;
using Vge.Util;

namespace Vge.Management
{
    /// <summary>
    /// Объект управления пользователями на сервере
    /// </summary>
    public class PlayerManager
    {
        /// <summary>
        /// Объект игрока владельца
        /// </summary>
        public PlayerServer PlayerOwner { get; private set; }
        /// <summary>
        /// Основной сервер
        /// </summary>
        public readonly GameServer Server;

        /// <summary>
        /// Менеджер команд
        /// </summary>
        private ManagerCommand _managerCommand;
        /// <summary>
        /// Колекция сетевых игроков
        /// </summary>
        private readonly List<PlayerServer> _players = new List<PlayerServer>();
        /// <summary>
        /// Список игроков которые надо запустить в ближайшем такте
        /// </summary>
        private readonly DoubleList<PlayerServer> _playerStartList = new DoubleList<PlayerServer>();
        /// <summary>
        /// Список сетевых игроков котрые надо выгрузить в ближайшем такте
        /// </summary>
        private readonly DoubleList<int> _playerRemoveList = new DoubleList<int>();
        /// <summary>
        /// Массив кэша удалённых игроков в тике для защиты перезахода
        /// </summary>
        private readonly ListFast<int> _cachePlayerRemoveList = new ListFast<int>(10);

        public PlayerManager(GameServer server)
        {
            Server = server;
            _managerCommand = new ManagerCommand(server);
        }

        #region AddRemoveCount

        /// <summary>
        /// Количество сетевый игроков
        /// </summary>
        public int PlayerNetCount() => _players.Count;

        /// <summary>
        /// Получить строку всех сетевых игроков
        /// </summary>
        public string ToStringPlayersNet() => string.Join("; ", _players);

        /// <summary>
        /// Добавить игрока владельца
        /// </summary>
        public void PlayerOwnerAdd(string login, string token)
        {
            PlayerOwner = Server.ModServer.CreatePlayerServer(login, token, null);

            Server.Log.Server(Srl.ServerLoginStartOwner, login);
            // Проверка токена не важна для владельца
            _GetPlayerData(PlayerOwner);
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
                playerServer.CauseRemove = cause;
                _playerRemoveList.Add(playerServer.Id);
                if (playerServer.Owner)
                {
                    _PlayerLeftGame(playerServer);
                    Server.Stop();
                }
                else
                {
                    SendToAllMessage(ChatStyle.Red + string.Format(L.S("PlayerRemove{0}{1}"), 
                        playerServer.Login, cause));
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
                return PlayerOwner;
            }
            foreach (PlayerServer player in _players)
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
            if (PlayerOwner != null && PlayerOwner.Id == id)
            {
                // Это владелец
                return PlayerOwner;
            }
            // Проверка сетевых
            foreach (PlayerServer player in _players)
            {
                if (player.Id == id)
                {
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// Найти объект Игрока по имени, если нет вернёт null
        /// </summary>
        public PlayerServer FindPlayerByLogin(string login)
        {
            if (PlayerOwner != null && PlayerOwner.Login == login)
            {
                // Это владелец
                return PlayerOwner;
            }
            // Проверка сетевых
            foreach (PlayerServer player in _players)
            {
                if (player.Login == login)
                {
                    return player;
                }
            }
            return null;
        }

        #endregion

        #region Send & All

        /// <summary>
        /// Удалить всех игроков при остановки сервера
        /// </summary>
        public void PlayersRemoveStopingServer()
        {
            if (_players.Count > 0)
            {
                for (int i = 0; i < _players.Count; i++)
                {
                    PlayerRemove(_players[i], Sr.StopServer);
                }
            }
            if (PlayerOwner != null)
            {
                PlayerRemove(PlayerOwner, Sr.StopServer);
            }
            _UpdateRemovingPlayers();
        }

        /// <summary>
        /// Отправить всем игрокам пакет, которые используют этот TileEntity
        /// </summary>
        public void SendToAllUseTileEntity(ITileEntity tileEntity, IPacket packet)
        {
            if (tileEntity != null)
            {
                // Основной игрок
                if (tileEntity.CheckEquals(PlayerOwner.Inventory.GetTileEntity()))
                {
                    Server.ResponsePacketOwner(packet);
                }
                // Сетевые игроки
                foreach (PlayerServer player in _players)
                {
                    if (tileEntity.CheckEquals(player.Inventory.GetTileEntity()))
                    {
                        player.SendPacket(packet);
                    }
                }
            }
        }

        /// <summary>
        /// Отправить пакет всем игрокам
        /// </summary>
        public void SendToAll(IPacket packet)
        {
            Server.ResponsePacketOwner(packet);
            foreach (PlayerServer player in _players)
            {
                player.SendPacket(packet);
            }
        }

        /// <summary>
        /// Отправить всем сообщение
        /// </summary>
        public void SendToAllMessage(string message)
            => SendToAll(new PacketS3AMessage(message));

        /// <summary>
        /// Получаем пакет сообщения от клиента
        /// </summary>
        /// <param name="player">Игрок который отправил, null значит через командную сервера</param>
        /// <param name="packet">Пакет сообщения</param>
        public void ClientMessage(PlayerServer player, PacketC14Message packet)
        {
            if (player != null)
            {
                string message = packet.GetCommandSender().GetMessage();
                Server.Log.Server("<{0}>: {1}", player.Login, message);
                if (message != "" && message[0] == '/')
                {
                    // Команды
                    message = _managerCommand.ExecutionCommand(packet.GetCommandSender().SetPlayer(player));
                    player.SendMessage(message);
                }
                else
                {
                    // Сообщение
                    SendToAllMessage(ChatStyle.Aqua + "[" + player.Login + "] " + ChatStyle.Reset + message);
                }
            }
        }

        #endregion

        #region SentPacket

        /// <summary>
        /// Пакет проверки сетевого логина игрока
        /// </summary>
        public void LoginStart(SocketSide socketSide, PacketC02LoginStart packet)
        {
            string login = packet.Login;
            if (packet.Version != Ce.IndexVersion)
            {
                // Клиент другой версии
                Server.Log.Server(Srl.ServerVersionAnother, login, packet.Version);
                socketSide.SendPacket(new PacketS02LoadingGame(PacketS02LoadingGame.EnumStatus.VersionAnother));
                return;
            }

            // Проверяем корректность никнейма
            if (login == "") // Тут можно указать перечень не корректных ников
            {
                Server.Log.Server(Srl.ServerLoginIncorrect, login);
                socketSide.SendPacket(new PacketS02LoadingGame(PacketS02LoadingGame.EnumStatus.LoginIncorrect));
                return;
            }

            // Проверяем имеется ли такой игрок с таким же именем по uuid в сети
            string uuid = PlayerServer.GetHash(login);
            foreach (PlayerServer player in _players)
            {
                if (uuid.Equals(player.UUID))
                {
                    Server.Log.Server(Srl.ServerLoginDuplicate, login);
                    socketSide.SendPacket(new PacketS02LoadingGame(PacketS02LoadingGame.EnumStatus.LoginDuplicate));
                    return;
                }
            }

            // Создаём объект Игрока
            PlayerServer playerServer = Server.ModServer.CreatePlayerServer(login, packet.Token, socketSide);

            // Загружаем данные игрока если имеются
            if (!_GetPlayerData(playerServer))
            {
                // Проверка токена не прошла
                Server.Log.Server(Srl.ServerInvalidToken, login, playerServer.Token);
                socketSide.SendPacket(new PacketS02LoadingGame(PacketS02LoadingGame.EnumStatus.InvalidToken));
                return;
            }

            // Передать готовность игры
            socketSide.SendPacket(new PacketS05Ready());

            // Поставить в очередь на cоединение сетевого игрока
            _playerStartList.Add(playerServer);
        }

        /// <summary>
        /// Поставить в очередь на cоединение игрока владельца
        /// </summary>
        public void JoinGameOwner() => _playerStartList.Add(PlayerOwner);

        #endregion

        #region Updates

        /// <summary>
        /// В тике на сервере подключаем и выкидываем игроков
        /// </summary>
        public void UpdateJoinLeftPlayers()
        {
            // Удаляем игроков
            _UpdateRemovingPlayers();
            // Добавляем игроков
            if (!_playerStartList.Empty())
            {
                _playerStartList.Step();
                int count = _playerStartList.CountBackward;
                for (int i = 0; i < count; i++)
                {
                    _PlayerJoinGame(_playerStartList.GetNext());
                }
                _cachePlayerRemoveList.Clear();
            }
        }

        /// <summary>
        /// Тик на сервере
        /// </summary>
        public void Update()
        {
            int countPl = PlayerOwner != null ? 1 : 0;
            // Только сетевые игроки
            for (int i = 0; i < _players.Count; i++)
            {
                countPl++;
                _PlayerServerUpdateTimeOut(_players[i]);
            }
            // Отладка
            if (Ce.IsDebugDrawChunks)
            {
                Debug.Players = new WinGL.Util.Vector2i[countPl];
                countPl = 0;
                if (PlayerOwner != null)
                {
                    Debug.Players[countPl++] = PlayerOwner.GetChunkPosition();
                }
                for (int i = 0; i < _players.Count; i++)
                {
                    Debug.Players[countPl++] = _players[i].GetChunkPosition();
                }
            }
        }

        /// <summary>
        /// Обновление раз в тик на сервере для проверки таймаута
        /// </summary>
        private void _PlayerServerUpdateTimeOut(PlayerServer entityPlayer)
        {
            // Проверка времени игрока без пинга, если игрок не отвечал больше 30 секунд
            if (!entityPlayer.Owner && entityPlayer.TimeOut())
            {
                // На сервере пометка убрать
                PlayerRemove(entityPlayer, Sr.TimeOut);
            }
        }

        /// <summary>
        ///  Удаляем игроков в тике
        /// </summary>
        private void _UpdateRemovingPlayers()
        {
            if (!_playerRemoveList.Empty())
            {
                _playerRemoveList.Step();
                int id;
                int count = _playerRemoveList.CountBackward;
                _cachePlayerRemoveList.Clear();
                for (int i = 0; i < count; i++)
                {
                    id = _playerRemoveList.GetNext();
                    _cachePlayerRemoveList.Add(id);
                    _PlayerLeftGame(FindPlayerById(id));
                }
            }
        }

        /// <summary>
        /// Подключаем игрока в игру, прошли все проверки на игрока
        /// </summary>
        private void _PlayerJoinGame(PlayerServer player)
        {
            if (player != null && !_cachePlayerRemoveList.Contains(player.Id))
            {
                if (!player.Owner)
                {
                    // Если это не владелец то добавляем в массив сетевых игроков
                    Server.Log.Server(Srl.ServerLoginStart, player.Login, player.Socket);
                    SendToAllMessage(ChatStyle.Yellow + string.Format(L.S("PlayerEntry{0}"), player.Login));
                    // Отправить всем игрокам, чтоб добавить в чат
                    SendToAll(new PacketS06PlayerEntryRemove(player.Id, player.Login));
                    _players.Add(player);

                    // Отправить этому игроку владельца
                    if (PlayerOwner != null)
                    {
                        player.SendPacket(new PacketS06PlayerEntryRemove(PlayerOwner.Id, PlayerOwner.Login));
                    }
                    // Отправить этому игроку всех игроков которые уже играют
                    for (int i = 0; i < _players.Count; i++)
                    {
                        player.SendPacket(new PacketS06PlayerEntryRemove(_players[i].Id, _players[i].Login));
                    }
                }
                else
                {
                    // Если владелец, просто добавим себя в списке чата, на случай если включится сеть
                    // Можно по сути и не вносить...
                    PlayerOwner.SendPacket(new PacketS06PlayerEntryRemove(PlayerOwner.Id, PlayerOwner.Login));
                }
                player.GetWorld().SpawnEntityInWorld(player);
                player.JoinGame();
            }
        }

        /// <summary>
        /// Вышел игрок с игры
        /// </summary>
        private void _PlayerLeftGame(PlayerServer player)
        {
            if (player != null)
            {
                if (!player.Owner)
                {
                    // Если это не владелец то добавляем в массив сетевых игроков
                    _players.Remove(player);
                    if (player.Socket.IsConnect())
                    {
                        // Сокет не закрыт, значит закрытие на стороне сервера
                        Server.PlayerDisconnect(player.Socket, player.CauseRemove);
                    }
                    // Отправить всем игрокам, чтоб убрать с чата
                    SendToAll(new PacketS06PlayerEntryRemove(player.Id));
                }
                player.SetDead();
                player.LeftGame();
            }
        }

        #endregion

        #region WriteReadSpawn

        /// <summary>
        /// Получить данные игрока, возращает false если токены разные
        /// </summary>
        private bool _GetPlayerData(PlayerServer entityPlayer)
        {
            string token = entityPlayer.Token;
            // Прочесть данные игрока с файла, возращает true если файл существола
            if (entityPlayer.ReadFromFile())
            {
                // Игрок загружен
                // Проверка токенов
                return token.Equals(entityPlayer.Token);
            }
            // Игрок впервые зашёл, создаём
            _CreatePlayer(entityPlayer);
            return true;
        }

        /// <summary>
        /// Создать игрока, тут первый спавн игрока
        /// </summary>
        private void _CreatePlayer(PlayerServer entityPlayer)
        {
            //TODO::2024-10-03 Создать игрока, тут первый спавн игрока
        }

        #endregion
    }
}
