using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeMasterServer {
    public class PacketHandler {

        public static void RecievePacket(ClientState state, byte[] packet) {
            Console.WriteLine("Recieved packet from client [id={0}]: (length={1}) {2}", state.Id, packet.Length, Util.ReadableBytes(packet));

            if (packet.Length < 3) return;

            // Third byte is always the packet id
            byte packetId = packet[2];

            switch(packetId) {
                case 208:
                    byte tokenLength = Math.Min(packet[3], (byte) 16); // Limits token to 16 bytes
                    byte[] token = packet.Skip(4).Take(16).ToArray();
                    List<byte> response = new List<byte>() { (byte) (tokenLength + 6), 0, 208, 0, 0, 0, 0, 0 };
                    response.AddRange(token);
                    SendPacket(state, response.ToArray());
                    break;

                case 209:
                    response = new List<byte>() { 1, 0, 209, (byte) Config.GameServers.Count, 0 };

                    int maxNameLength = 13;
                    int maxIpLength = 7;
                    for (int i = 0; i < Config.GameServers.Count; i++) {
                        // Creates a new array filled with 0s
                        byte[] bytes = new byte[maxNameLength + maxIpLength];

                        string name = Config.GameServers.ElementAt(i).Key;
                        byte[] nameBytes = Encoding.ASCII.GetBytes(name).Take(maxNameLength).ToArray();
                        nameBytes.CopyTo(bytes, 0);

                        string ip = Config.GameServers.ElementAt(i).Value;
                        IPAddress address = IPAddress.Parse(ip);
                        byte[] addressBytes = address.GetAddressBytes().Take(7).ToArray();
                        addressBytes.CopyTo(bytes, maxNameLength);

                        response.AddRange(bytes);
                    }

                    SendPacket(state, response.ToArray());
                    state.Close();
                    break;
            }
        }

        public static void SendPacket(ClientState state, byte[] packet) {
            Console.WriteLine("Sent packet to client [id={0}]: (length={1}) {2}", state.Id, packet.Length, Util.ReadableBytes(packet));

            state.ClientSocket.BeginSend(packet, 0, packet.Length, 0, null, state);
        }
    }
}
