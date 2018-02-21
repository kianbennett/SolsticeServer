using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class SvcPacketHandler {

        public static void RecievePacket(ClientState state, byte[] packet) {
            if (packet.Length > 3 && packet[2] == 5) return; // Don't print or handle keep alive packets
            Console.WriteLine("[SVC] Recieved packet from client [id={0}]: (length={1}) {2}", state.Id, packet.Length, Util.ReadableBytes(packet));

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
                    Character charNew = Character.FromBytes(packet.Skip(3).ToArray());
                    SqlHelper.SaveCharacter(charNew, state.Username);
                    SendCharacters(state);
                    break;
                case 214: // Delete character
                    byte charId = packet[3];
                    SqlHelper.DeleteCharacter(state.Username, charId);
                    SendCharacters(state);
                    break;
                case 215: // Load into game
                    SendPacket(state, new byte[] { 1, 0, 215 });
                    break;
                case 5: // Keep alive packet (ignore for now)
                    break;
            }
        }

        public static void SendCharacters(ClientState state) {
            Character[] characters = SqlHelper.GetCharactersFromUsername(state.Username);
            int responseLength = 43 * characters.Length + 6;
            List<byte> response = new List<byte>() { (byte) responseLength, 0, 212, 0, 0, 0, 0, (byte) characters.Length };
            foreach (Character c in characters) {
                response.AddRange(c.ToBytes());
            }
            SendPacket(state, response.ToArray());
        }

        public static void SendPacket(ClientState state, byte[] packet) {
            Console.WriteLine("[SVC] Sent packet to client [id={0}]: (length={1}) {2}", state.Id, packet.Length, Util.ReadableBytes(packet));
            state.ClientSocket.BeginSend(packet, 0, packet.Length, 0, null, state);
        }
    }
}
