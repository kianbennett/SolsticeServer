using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class WorldPacketHandler {

        public static void RecievePacket(ClientState state, byte[] packet) {
            if (packet.Length > 3 && packet[2] == 5) return; // Don't print or handle keep alive packets
            Console.WriteLine("[World] Recieved packet from client [id={0}]: (length={1}) {2}", state.Id, packet.Length, Util.ReadableBytes(packet));

            if (packet.Length < 3) return;

            byte packetId = packet[2];

            switch (packetId) {
                case 191:
                    SendPacket(state, new byte[] { 7, 0, 176, 0, 0, 0, 0, 0, 18 }); // enables map loading
                    SendPacket(state, new byte[] { 15, 0, 176, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 0, 0, 0 }); // Sets map
                    SendPacket(state, new byte[] { 7, 0, 176, 0, 0, 0, 0, 0, 19 }); // loads map


                    SendPacket(state, new byte[] { // Send player data
                        181, 0, 192,
                        0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 13, 20, 15, 16, 17, 18,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                    });
                    break;
                case 5: // Keep alive packet (ignore for now)
                    break;
            }
        }

        public static void SendPacket(ClientState state, byte[] packet) {
            Console.WriteLine("[World] Sent packet to client [id={0}]: (length={1}) {2}", state.Id, packet.Length, Util.ReadableBytes(packet));
            state.ClientSocket.BeginSend(packet, 0, packet.Length, 0, null, state);
        }
    }
}
