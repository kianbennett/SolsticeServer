using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class ChatMessagePacketHandler {

        public static void HandleChatMessage(ClientState client, byte[] packet) {
            string msg = Encoding.ASCII.GetString(packet.Skip(10).ToArray());
            switch(packet[8]) {
                case 17: // General chat
                    Console.WriteLine("[Message] {0}: {1}", client.Username, msg);

                    if(msg.StartsWith("!")) {
                        msg = msg.Substring(1);
                        string[] split = msg.Split(" ".ToCharArray());
                        switch(split[0]) {
                            case "h":
                                SendSystemMessage(client, "Server commands: !setmap <id>, !setpos <x> <y>, !levelup, !setlevel <lvl>, !additem <id>, !echo <msg>");
                                break;
                            case "setmap":
                                short id = 0;
                                if(split.Length >= 2 && short.TryParse(split[1], out id)) {
                                    if(World.MapList.ContainsKey(id)) {
                                        client.LoadMap(id);
                                    } else {
                                        SendSystemMessage(client, "No map with ID " + id + ".");
                                    }
                                } else {
                                    SendSystemMessage(client, "Usage: !setmap <id>");
                                }
                                break;
                            case "setpos":
                                short x = 0, y = 0;
                                if(split.Length >= 3 && short.TryParse(split[1], out x) && short.TryParse(split[2], out y)) {
                                    if(client.PlayerObject != null) client.PlayerObject.SetPosition(x, y);
                                } else {
                                    SendSystemMessage(client, "Usage: !setpos <x> <y>");
                                }
                                break;
                            case "levelup":
                                client.PlayerObject.LevelUp();
                                break;
                            case "setlevel":
                                byte level = 0;
                                if (split.Length >= 2 && byte.TryParse(split[1], out level)) {
                                    client.PlayerObject.SetLevel(level, true);
                                } else {
                                    SendSystemMessage(client, "Usage: !setlevel <lvl>");
                                }
                                break;
                            case "additem":
                                short itemId = 0;
                                if (split.Length >= 2 && short.TryParse(split[1], out itemId)) {
                                    client.PlayerObject.AddItem(new Item(itemId, 1));
                                } else {
                                    SendSystemMessage(client, "Usage: !additem <id>");
                                }
                                break;
                            case "addkron":
                                int kron = 0;
                                if (split.Length >= 2 && int.TryParse(split[1], out kron)) {
                                    client.PlayerObject.SetKron(client.PlayerData.Kron + kron);
                                } else {
                                    SendSystemMessage(client, "Usage: !addkron <kron>");
                                }
                                break;
                            case "setkron":
                                if (split.Length >= 2 && int.TryParse(split[1], out kron)) {
                                    client.PlayerObject.SetKron(kron);
                                } else {
                                    SendSystemMessage(client, "Usage: !setkron <kron>");
                                }
                                break;
                            case "echo":
                                if(split.Length >= 2) SendSystemMessage(client, msg.Substring(5));
                                    else SendSystemMessage(client, "Usage: !echo <msg>");
                                break;
                            case "ping": // TODO: Find a packet that, when sent from the server to the client, will cause the client to send back a packet immediately
                                break;
                            default:
                                SendSystemMessage(client, "Invalid command.");
                                break;
                        }
                    } else {
                        SendUserMessage(client, client.Username, msg);
                    }
                    break;
            }
        }

        public static void SendSystemMessage(ClientState client, string msg) {
            if(msg.Length > 255) msg = msg.Substring(0, 255); // Limit string length to 1 byte
            byte[] packet = Util.PacketWithId((short) (8 + msg.Length), 186);
            packet[8] = 160;
            packet[9] = (byte) msg.Length;
            Encoding.ASCII.GetBytes(msg).CopyTo(packet, 10);
            WorldPacketHandler.SendPacket(client, packet);
        }

        public static void SendUserMessage(ClientState client, string user, string msg) {
            if (msg.Length > 255) msg = msg.Substring(0, 255); // Limit string length to 1 byte
            byte[] packet = Util.PacketWithId((short) (9 + msg.Length + user.Length), 186);
            packet[8] = 31;
            packet[9] = (byte) user.Length;
            packet[10] = (byte) msg.Length;
            Encoding.ASCII.GetBytes(user).CopyTo(packet, 11);
            Encoding.ASCII.GetBytes(msg).CopyTo(packet, 11 + user.Length);
            WorldPacketHandler.SendPacketToAllOnMap(World.MapList[client.PlayerObject.MapId], null, packet);
        }
    }
}
