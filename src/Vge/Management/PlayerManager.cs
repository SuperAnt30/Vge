using System.Collections.Generic;
using Vge.Games;
using Vge.Network;

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
        /// Колекция сокетов клиентов
        /// </summary>
        private List<PlayerServer> players = new List<PlayerServer>();
        //private PlayerServer playerOwner;

        public PlayerManager(Server server)
        {
            this.server = server;
            
           // playerOwner = new PlayerServer(null, server);
        }


        /// <summary>
        /// TODO::TestUserAllKill
        /// </summary>
        public void TestUserAllKill()
            => AllNetPlayersDisconnect(Sr.ThrownOut);

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

        public void PlayerAdd(SocketSide socketSide)
        {
            players.Add(new PlayerServer(socketSide, server));
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

        /// <summary>
        /// Количество сетевый игроков
        /// </summary>
        public int PlayerNetCount() => players.Count;

    }
}
