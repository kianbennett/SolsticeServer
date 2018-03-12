using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class ChatPacketHandler {

        public static void RecievePacket(ClientState client, byte[] packet) {
            if (packet.Length > 2 && packet[2] == 5) return; // Don't print or handle keep alive packets

            Console.WriteLine("[Chat] client -> server [id={0}]: (length={1}) {2}", client.Id, packet.Length, Util.ReadableBytes(packet));

            if (packet.Length < 3) return;
            byte packetId = packet[2];

            switch (packetId) {
                case 176: // Set name
                    byte[] name = packet.Skip(4).ToArray();
                    client.Username = Encoding.ASCII.GetString(name); // This is the character name, not the username

                    List<string> buddyList = SqlHelper.GetBuddyList(client.Username);
                    List<byte> buddyListPacket = new List<byte>();
                    foreach(string buddy in buddyList) {
                        buddyListPacket.AddRange(AddBuddyPacket(client, Encoding.ASCII.GetBytes(buddy)));
                    }
                    SendPacket(client, buddyListPacket.ToArray());
                    client.PlayerData = new PlayerData() {
                        BuddyList = buddyList
                    };

                    SendStatus(client, 1);

                    //SendPacket(state, new byte[] { 13, 0, 185, 0, 0, 0, 0, 0, 2, 0, 4, 70, 82, 78, 49 }); // Add player FRN1 to buddy list (group id = 0)
                    //SendPacket(state, new byte[] { 13, 0, 185, 0, 0, 0, 0, 1, 1, 0, 4, 70, 82, 78, 50 }); // Add player FRN2 to buddy list (group id = 1)
                    //SendPacket(state, new byte[] { 13, 0, 185, 0, 0, 0, 0, 1, 1, 0, 4, 70, 82, 78, 51 }); // Add player FRN3 to buddy list (group id = 1)

                    //SendPacket(state, new byte[] { 7, 0, 191, 1, 4, 71, 82, 80, 49 });  // Create group GRP1 (id = 1)
                    //SendPacket(state, new byte[] { 7, 0, 191, 2, 4, 71, 82, 80, 50 });  // Create group GRP2 (id = 2)

                    //SendPacket(state, new byte[] { 9, 0, 190, 3, 3, 65, 66, 67, 68, 69, 70 });  // Recieved message DEF from ASD
                    //SendPacket(state, new byte[] { 11, 0, 180, 0, 0, 0, 0, 1, 4, 70, 82, 78, 49 });   // Set status of friend (id to 179 to show "has logged-in" notification)

                    break;
                case 181: // Block/unblock buddy
                    byte blocked = packet[3];
                    byte[] nameBytes = packet.Skip(5).ToArray();
                    SendBuddyBlocked(client, nameBytes, blocked);
                    break;
                case 182: // Set status
                    byte status = packet[3];
                    SendStatus(client, status);
                    break;
                case 183: // Send buddy request
                    byte nameLength = packet[3];
                    byte targetLength = packet[4];
                    byte msgLength = packet[5];
                    byte[] targetName = packet.Skip(6 + nameLength).Take(targetLength).ToArray();

                    if (client.PlayerData.BuddyList.Contains(Encoding.ASCII.GetString(targetName))) {
                        SendNotice(client, 7);
                    } else {
                        SendBuddyRequest(client, targetName, packet.Skip(6 + nameLength + targetLength).Take(msgLength).ToArray());
                    }
                    break;
                case 184: // Response to buddy request
                    byte response = packet[3]; // 0 = no, 1 = yes
                    name = packet.Skip(5).ToArray();
                    SendBuddyRequestResponse(client, response, name);
                    break;
                case 186: // Delete buddy
                    nameBytes = packet.Skip(4).ToArray(); // TODO: check if name is in buddy list, if not send error notice
                    string nameStr = Encoding.ASCII.GetString(nameBytes);
                    if(client.PlayerData.BuddyList.Contains(nameStr)) {
                        client.PlayerData.BuddyList.Remove(nameStr);
                        SqlHelper.SaveBuddyList(client.Username, client.PlayerData.BuddyList);
                        SendDeleteBuddy(client, nameBytes);
                    } else {
                        SendNotice(client, 9);
                    }
                    break;
                case 192: // Delete group
                    byte groupId = packet[3];
                    SendDeleteGroup(client, groupId); // TODO: check that group is empty before deleting it
                    break;
                case 193: // Rename group
                    groupId = packet[3];
                    nameBytes = packet.Skip(5).ToArray();
                    SendRenameGroup(client, groupId, nameBytes);
                    break;
                case 194:
                    groupId = packet[3];
                    nameBytes = packet.Skip(5).ToArray();
                    SendAddBuddyToGroup(client, groupId, nameBytes);
                    break;
            }
        }

        public static void SendStatus(ClientState state, byte status) {
            SendPacket(state, new byte[] { 2, 0, 182, status });
        }

        public static void SendNotice(ClientState state, byte id) {
            SendPacket(state, new byte[] { 2, 0, 2, id });
        }

        public static void SendBuddyRequest(ClientState client, byte[] targetName, byte[] msg) {
            ClientState[] clients = ChatServer.ClientList.Where(o => o.Username == Encoding.ASCII.GetString(targetName)).ToArray();
            if(clients.Length == 0) {
                SendNotice(client, 4); // Failed to add a buddy
                return;
            }
            ClientState targetClient = clients[0];
            byte[] packet = Util.PacketWithId((short) (3 + client.Username.Length + msg.Length), 183);
            packet[3] = (byte) client.Username.Length;
            packet[4] = (byte) msg.Length;
            Encoding.ASCII.GetBytes(client.Username).CopyTo(packet, 5);
            msg.CopyTo(packet, 5 + client.Username.Length);
            SendPacket(targetClient, packet);
        }

        public static void SendBuddyRequestResponse(ClientState client, byte success, byte[] targetName) {
            ClientState targetClient = ChatServer.GetClientFromName(Encoding.ASCII.GetString(targetName));
            if(targetClient == null) {
                SendNotice(client, 4); // This buddy does not exist
                return;
            }

            byte[] packet = Util.PacketWithId((short) (3 + client.Username.Length), 184);
            packet[3] = success;
            packet[4] = (byte) client.Username.Length;
            Encoding.ASCII.GetBytes(client.Username).CopyTo(packet, 5);

            if (success == 0) {
                SendPacket(targetClient, packet);
            } else {
                SendPacket(targetClient, Util.CombinePackets(packet, AddBuddyPacket(client, Encoding.ASCII.GetBytes(client.Username).ToArray())));

                packet = Util.PacketWithId((short) (3 + targetName.Length), 184);
                packet[3] = success;
                packet[4] = (byte) targetName.Length;
                targetName.CopyTo(packet, 5);

                SendPacket(client, Util.CombinePackets(packet, AddBuddyPacket(client, targetName)));
                client.PlayerData.BuddyList.Add(Encoding.ASCII.GetString(targetName));
                SqlHelper.SaveBuddyList(client.Username, client.PlayerData.BuddyList);
            }
        }

        public static byte[] AddBuddyPacket(ClientState client, byte[] name) {
            byte[] packet = Util.PacketWithId((short) (9 + name.Length), 185);
            packet[7] = 0; // group ID
            packet[8] = 1; // status
            packet[10] = (byte) name.Length;
            name.CopyTo(packet, 11);
            return packet;
        }

        public static void SendDeleteBuddy(ClientState state, byte[] name) {
            byte[] packet = new byte[name.Length + 4];
            BitConverter.GetBytes(packet.Length - 2).Take(2).ToArray().CopyTo(packet, 0);
            packet[2] = 187;
            packet[3] = (byte) name.Length;
            name.CopyTo(packet, 4);
            SendPacket(state, packet);
        }
         
        public static void SendDeleteGroup(ClientState state, byte groupId) {
            // TODO: when group is deleted, move all buddies in the group out of the group
            SendPacket(state, new byte[] { 2, 0, 192, groupId });
        }

        public static void SendRenameGroup(ClientState state, byte groupId, byte[] name) {
            byte[] packet = new byte[name.Length + 5];
            BitConverter.GetBytes(packet.Length - 2).Take(2).ToArray().CopyTo(packet, 0);
            packet[2] = 193;
            packet[3] = groupId;
            packet[4] = (byte) name.Length;
            name.CopyTo(packet, 5);
            SendPacket(state, packet);
        }

        public static void SendAddBuddyToGroup(ClientState state, byte groupId, byte[] buddy) {
            byte[] packet = new byte[buddy.Length + 5];
            BitConverter.GetBytes(packet.Length - 2).Take(2).ToArray().CopyTo(packet, 0);
            packet[2] = 194;
            packet[3] = groupId;
            packet[4] = (byte) buddy.Length;
            buddy.CopyTo(packet, 5);
            SendPacket(state, packet);
        }

        public static void SendBuddyBlocked(ClientState state, byte[] buddy, byte blocked) {
            byte[] packet = new byte[buddy.Length + 5];
            BitConverter.GetBytes(packet.Length - 2).Take(2).ToArray().CopyTo(packet, 0);
            packet[2] = 181;
            packet[3] = BitConverter.GetBytes(blocked)[0];
            packet[4] = (byte) buddy.Length;
            buddy.CopyTo(packet, 5);
            SendPacket(state, packet);
        }

        public static void SendPacket(ClientState state, byte[] packet) {
            Console.WriteLine("[Chat] server -> client [id={0}]: (length={1}) {2}", state.Id, packet.Length, Util.ReadableBytes(packet));
            state.ClientSocket.BeginSend(packet, 0, packet.Length, 0, null, state);
        }
    }
}
