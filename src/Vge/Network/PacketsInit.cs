using Vge.Network.Packets;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;

namespace Vge.Network
{
    /// <summary>
    /// Класс создания пакетов
    /// </summary>
    public class PacketsInit
    {

      //  public static PacketS21ChunkData 

        /// <summary>
        /// Пакеты пришедшие с сервера
        /// </summary>
        public static IPacket InitServer(byte index)
        {
            switch (index)
            {
                case 0x00: return new Packet00PingPong();
                case 0x01: return new Packet01KeepAlive();
                case 0x02: return new PacketS02LoadingGame();
                case 0x03: return new PacketS03JoinGame();
                case 0x04: return new PacketS04TimeUpdate();
                case 0x05: return new PacketS05TableBlocks();
                case 0x07: return new PacketS07RespawnInWorld();
                case 0x08: return new PacketS08PlayerPosLook();
                case 0x20: return new PacketS20ChunkSend();
                case 0x21: return new PacketS21ChunkData();
            }

            return null;
        }

        /// <summary>
        /// Пакеты пришедшие с клиента
        /// </summary>
        public static IPacket InitClient(byte index)
        {
            switch (index)
            {
                case 0x00: return new Packet00PingPong();
                case 0x01: return new Packet01KeepAlive();
                case 0x02: return new PacketC02LoginStart();
                case 0x04: return new PacketC04PlayerPosition();
                case 0x15: return new PacketC15PlayerSetting();
                case 0x20: return new PacketC20AcknowledgeChunks();
            }

            return null;
        }
    }
}
