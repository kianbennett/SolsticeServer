using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class SvcPacketHandler {

        public static void RecievePacket(ClientState state, byte[] packet) {
            if (packet.Length > 2 && packet[2] == 5) return; // Don't print or handle keep alive packets
            Console.WriteLine("[SVC] client -> server [id={0}]: (length={1}) {2}", state.Id, packet.Length, Util.ReadableBytes(packet));

            if (packet.Length < 3) return;
            byte packetId = packet[2];

            switch(packetId) {
                case 211: // Request login
                    byte[] token = packet.Skip(23).Take(16).ToArray();
                    string tokenStr = Encoding.ASCII.GetString(token);
                    state.Username = tokenStr.Trim((char) 0).Trim();
                    if (state.Username.Length == 0) {
                        Console.WriteLine("[SVC] Empty username from id={0}", state.Id);
                        SendPacket(state, new byte[] { 2, 0, 211, 4 }); // 'Failed to log-in'
                        state.Close();
                        break;
                    }
                    Console.WriteLine("[SVC] Client id={0} has set their username to '{1}'", state.Id, state.Username); // Takes username from token (temporary)
                    SendPacket(state, new byte[] { 2, 0, 211, 0 });
                    break;
                case 212: // Request characters
                    SendCharacters(state);
                    break;
                case 213: // Create character
                    PlayerData data = PlayerData.FromBytes(packet.Skip(3).ToArray());
                    if (data.Name.Contains(",")) return;
                    SqlHelper.SaveCharacter(data, state.Username);
                    SendCharacters(state);
                    break;
                case 214: // Delete character
                    byte charId = packet[3];
                    SqlHelper.DeleteCharacter(state.Username, charId);
                    SendCharacters(state);
                    break;
                case 215: // Load into game
                    SendPacket(state, new byte[] { 2, 0, 215, 0 });
                    break;
                case 5: // Keep alive packet (ignore for now)
                    break;
            }
        }

        public static void SendCharacters(ClientState state) {
            PlayerData[] characters = SqlHelper.GetCharactersFromUsername(state.Username);
            byte[] packet = Util.PacketWithId((short) (43 * characters.Length + 6), 212);
            IPAddress.Parse(Config.IpChat).GetAddressBytes().Take(4).ToArray().CopyTo(packet, 3);
            packet[7] = (byte) characters.Length;
            for(int i = 0; i < characters.Length; i++) {
                characters[i].SvcBytes().CopyTo(packet, 8 + 43 * i);
            }
            SendPacket(state, packet);
        }

        public static void SendPacket(ClientState state, byte[] packet) {
            Console.WriteLine("[SVC] server -> client [id={0}]: (length={1}) {2}", state.Id, packet.Length, Util.ReadableBytes(packet));
            state.ClientSocket.BeginSend(packet, 0, packet.Length, 0, null, state);
        }
    }
}
