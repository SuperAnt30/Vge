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
                case 0x06: return new PacketS06PlayerEntryRemove();
                case 0x07: return new PacketS07RespawnInWorld();
                case 0x08: return new PacketS08PlayerPosLook();
                case 0x0C: return new PacketS0CSpawnPlayer();
                case 0x13: return new PacketS13DestroyEntities();
                case 0x14: return new PacketS14EntityMotion();
                case 0x20: return new PacketS20ChunkSend();
                case 0x21: return new PacketS21ChunkData();
                case 0x22: return new PacketS22MultiBlockChange();
                case 0x23: return new PacketS23BlockChange();
                case 0x3A: return new PacketS3AMessage();
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
                case 0x07: return new PacketC07PlayerDigging();
                case 0x08: return new PacketC08PlayerBlockPlacement();
#if PhysicsServer
                case 0x0C: return new PacketC0CInput();
                case 0x0D: return new PacketC0DInputRotate();
#endif
                case 0x14: return new PacketC14Message();
                case 0x15: return new PacketC15PlayerSetting();
                case 0x20: return new PacketC20AcknowledgeChunks();
            }

            return null;
        }
    }
}
