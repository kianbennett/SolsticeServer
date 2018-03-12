using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class WorldPacketHandler {

        private static int i = 0;

        public static void RecievePacket(ClientState client, byte[] packet) {
            if (packet.Length < 3) return;
            if(!((packet[2] == 188 && packet[8] == 22) || packet[2] == 5))  // Don't print position or keep alive packets
                Console.WriteLine("[World] client -> server [id={0}]: (length={1}) {2}", client.Id, packet.Length, Util.ReadableBytes(packet));

            byte packetId = packet[2];

            switch (packetId) {
                case 191:
                    byte nameLength = packet[34];
                    if (nameLength <= 0) return;
                    string name = Encoding.ASCII.GetString(packet, 35, nameLength);
                    string username = Encoding.ASCII.GetString(packet, 17, 16).Trim((char) 0).Trim();
                    client.Username = username;

                    ChatMessagePacketHandler.SendSystemMessage(client, "Welcome, " + name + ". Type !h for server commands.");

                    client.PlayerData = SqlHelper.GetPlayerDataFromName(username, name);
                    client.PlayerData.MapId = 84;
                    client.PlayerData.PosX = 63;
                    client.PlayerData.PosY = 30;
                    client.LoadMap(client.PlayerData.MapId);

                    SendPacket(client, new PlayerObject(null, 10, new PlayerData() {
                        Name = "player2",
                        PosX = 67,
                        PosY = 32,
                        Direction = 2
                    }).SpawnPlayerPacket(false, false));

                    //client.PlayerObject.ActivePet = new PetObject(0, 1, 61, 30, 2);
                    //SendPacket(client, client.PlayerObject.ActivePet.SpawnPetPacket(false));

                    //SendPacket(state, new byte[] { 11, 0, 195, 0, 0, 0, 0, 0, 1, (byte) WorldServer.ClientList.Count, 0, 1, 0 }); // Num players on server
                    //SendPacket(state, new byte[] { 9, 0, 195, 0, 0, 0, 0, 0, 6, (byte) WorldServer.ClientList.Count, 0 }); // Num players on map

                    //SendPacket(state, new byte[] { 13, 0, 191, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 }); // Sets player id to 0

                    //SendPacket(state, new byte[] { 15, 0, 176, 0, 0, 0, 0, 0, 17, 19, 7, 0, 0, 0, 0, 0, 0 }); // Sets map
                    //SendMap(state, 84, 63, 30);

                    // Spawns player
                    //SendPacket(state, new byte[] { 45, 0, 177, 0, 0, 0, 0, 0, 1,
                    //    0, 0, 0, 20, 0, 0, 0, 0,
                    //    0, 0, 0, 0, 0, 0, 20, 2,
                    //    0, 0, 63, 0, 30, 0, 0, 0,
                    //    0, 0, 0, 0, 0, 0, 3, 3,
                    //    65, 66, 67, 68, 69, 70
                    //});

                    //SendPlayerStats(state);

                    //SendSkillList(state, 15, 25, 35, 45);
                    //SendQuestList(state, 1, 2, 3, 4);

                    //SendPlayerInventory(state, 0,
                    //    new Item(500, 1),
                    //    new Item(501, 1),
                    //    new Item(502, 1)
                    //);

                    //SendPlayerInventory(state, 1, 
                    //    new Item(1190, 1),
                    //    new Item(1191, 1),
                    //    new Item(1192, 1),
                    //    new Item(1250, 1),
                    //    new Item(1201, 1),
                    //    new Item(1202, 1),
                    //    new Item(536, 1)
                    //);

                    //SendPlayerInventory(state, 2,
                    //    new Item(6, 1),
                    //    new Item(7, 1),
                    //    new Item(8, 1)
                    //);

                    //Item[] items = new Item[255];
                    //for (int i = 0; i < items.Length; i++) items[i] = new Item((short) (i + 500), 1);
                    //SendPlayerInventory(state, 0, items);

                    //SendPickedUpItem(state, new Item(1193, 2));

                    //SendPacket(state, new byte[] { 7, 0, 176, 0, 0, 0, 0, 0, 19 }); // loads map

                    //SendPacket(state, new byte[] {        // attack target
                    //    26, 0, 181, 0, 0, 0, 0, 0,
                    //    177,    // packet id
                    //    1,      // obj type (player)
                    //    0, 0,      // obj id
                    //    1,      // target type
                    //    0, 0,   // target id
                    //    0, 1,   // attack speed
                    //    2, 2, 0,    // number of attacks = 2 + 2 (0 is ?)
                    //    3, 0, 4, 0, // damage of each attach
                    //    7, 0, 8, 0
                    //});

                    break;
                case 186: // Typed in chat box
                    ChatMessagePacketHandler.HandleChatMessage(client, packet);
                    break;
                case 188:
                    switch (packet[8]) {
                        case 17: // Sit
                            if (client.PlayerObject.Stance == 0) client.PlayerObject.SetStance(1);
                            break;
                        case 18: // Stand
                            if (client.PlayerObject.Stance == 1) client.PlayerObject.SetStance(0);
                            break;
                        case 21: // Change direction
                            client.PlayerObject.SetDirection(packet[9]);
                            break;
                        case 22: // Set position
                            client.PlayerObject.SetPosition(BitConverter.ToInt16(packet, 9), BitConverter.ToInt16(packet, 11));
                            break;
                        case 23: // attack
                            byte targetType = packet[9];
                            short targetId = BitConverter.ToInt16(packet, 10);
                            SendPacket(client, client.PlayerObject.DamageTargetPacket(targetType, targetId, 1));
                            break;
                        case 25: // Show emote
                            byte emoteId = packet[9];
                            client.PlayerObject.ShowEmoticon(emoteId);
                            break;
                        case 35: // Equip item
                            byte slotId = packet[9];
                            SendEquippedItem(client, slotId, Item.FromBytes(packet.Skip(10).Take(8).ToArray()));
                            break;
                        case 41: // Equip mslot
                            SendEquippedMSlot(client, BitConverter.ToInt16(packet, 9));
                            break;
                        case 42: // Equip cosmetic
                            slotId = packet[9];
                            SendEquippedItem(client, slotId, Item.FromBytes(packet.Skip(10).Take(8).ToArray()), true);
                            break;
                        case 54: // Delete quest
                            short quest = BitConverter.ToInt16(packet, 9);
                            RemoveQuest(client, quest);
                            break;
                        case 65: // Add skill point
                            client.PlayerData.AddSkillPoint(client, (Skill) packet[9]);
                            break;
                    }
                    break;
                case 187: // open shop
                    short map = World.MapList[World.MapList.Keys.ElementAt(++i)].Id;
                    client.LoadMap(map);
                    Console.WriteLine("[World] Set map: " + map);
                    break;
                case 176: // quit game
                    SendPacket(client, new byte[] { 8, 0, 3, 0, 0, 0, 0, 0, 255, 15 });
                    client.Close();
                    break;
                case 196:
                    break;
                case 5: // Keep alive packet
                    client.IsAlive = true;
                    break;
            }
        }

        // 0 = spend, 1 = gear, 2 = other
        public static void SendPlayerInventory(ClientState state, int inventoryId, params Item[] items) {
            if(inventoryId < 0 || inventoryId > 2) {
                Console.WriteLine("Invalid inventory id");
                return;
            }

            List<byte> packet = new List<byte>() {
                192, 0, 0, 0, 0, 0, (byte) (49 + inventoryId), (byte) items.Length
            };
            short len = (short) (items.Length * 8 + 8);
            packet.InsertRange(0, BitConverter.GetBytes(len));

            foreach(Item item in items) {
                packet.AddRange(item.ToBytes());
            }

            SendPacket(state, packet.ToArray());
        }
         
        // Item gets placed automatically in the correct inventory
        public static void SendPickedUpItem(ClientState state, Item item) {
            byte[] packet = new byte[17];
            packet[0] = 15;
            packet[2] = 192;
            packet[8] = 52;
            item.ToBytes().CopyTo(packet, 9);
            SendPacket(state, packet);
        }

        public static void SendEquippedItem(ClientState state, int slotId, Item item, bool cosmetic = false) {
            if (item == null) item = new Item(0, 0); // if item is null set to empty
            byte[] packet = new byte[17];
            packet[0] = 15;
            packet[2] = 192;
            packet[8] = (byte) ((cosmetic ? 224 : 96) + slotId);
            item.ToBytes().CopyTo(packet, 9);
            SendPacket(state, packet);
        }

        public static void SendEquippedMSlot(ClientState state, short itemId) {
            byte[] packet = new byte[11];
            packet[0] = 9;
            packet[2] = 192;
            packet[8] = 110;
            BitConverter.GetBytes(itemId).CopyTo(packet, 9);
            SendPacket(state, packet);
        }

        public static void SendSkillList(ClientState state, params short[] skills) {
            byte[] packet = new byte[10 + 2 * skills.Length];
            BitConverter.GetBytes(packet.Length - 2).Take(2).ToArray().CopyTo(packet, 0);
            packet[2] = 192;
            packet[8] = 80;
            packet[9] = (byte) skills.Length;
            for(int i = 0; i < skills.Length; i++) {
                BitConverter.GetBytes(skills[i]).CopyTo(packet, 10 + 2 * i);
            }
            SendPacket(state, packet);
        }

        public static void SendQuestList(ClientState state, params short[] quests) {
            byte[] packet = new byte[10 + 2 * quests.Length];
            BitConverter.GetBytes(packet.Length - 2).Take(2).ToArray().CopyTo(packet, 0);
            packet[2] = 192;
            packet[8] = 160;
            packet[9] = (byte) quests.Length;
            for (int i = 0; i < quests.Length; i++) {
                BitConverter.GetBytes(quests[i]).CopyTo(packet, 10 + 2 * i);
            }
            SendPacket(state, packet);
        }

        public static void RemoveQuest(ClientState state, short quest) {
            byte[] packet = new byte[11];
            packet[0] = 9;
            packet[2] = 192;
            packet[8] = 162;
            BitConverter.GetBytes(quest).CopyTo(packet, 9);
            SendPacket(state, packet);
        }

        public static void SendOpenItemCharge(ClientState state, int gold, params Item[] items) {
            byte[] packet = new byte[18 + items.Length * 8];
            BitConverter.GetBytes(packet.Length - 2).Take(2).ToArray().CopyTo(packet, 0);
            packet[2] = 192;
            packet[8] = 128;
            BitConverter.GetBytes(gold).CopyTo(packet, 9);
            packet[17] = (byte) items.Length;
            for(int i = 0; i < items.Length; i++) {
                items[i].ToBytes().CopyTo(packet, 18 + i * 8);
            }

            SendPacket(state, packet);
        }

        public static void SendOpenCharInfo(ClientState state, string memId, byte[] ip, string charId, byte lvl, int map, params Item[] items) {
            byte[] packet = new byte[116 + memId.Length + charId.Length];
            BitConverter.GetBytes(packet.Length - 2).Take(2).ToArray().CopyTo(packet, 0);
            packet[2] = 195;
            packet[8] = 32;
            ip.Take(4).ToArray().CopyTo(packet, 9);
            BitConverter.GetBytes(map).CopyTo(packet, 13);
            for(int i = 0; i < 12; i++) {
                if (i >= items.Length) break;
                items[i].ToBytes().CopyTo(packet, 17 + i * 8);
            }
            packet[113] = lvl;
            packet[114] = (byte) charId.Length;
            packet[115] = (byte) memId.Length;
            Encoding.ASCII.GetBytes(charId).CopyTo(packet, 116);
            Encoding.ASCII.GetBytes(memId).CopyTo(packet, 116 + charId.Length);
            SendPacket(state, packet);
        }

        /* 1 = 'S', 2 = 'M', 3 = 'O', 4 = 'C' */
        public static void SendCharInfoItemList(ClientState state, byte itemListId, params Item[] items) {
            byte[] packet = new byte[10 + 8 * items.Length];
            BitConverter.GetBytes(packet.Length - 2).Take(2).ToArray().CopyTo(packet, 0);
            packet[2] = 195;
            packet[8] = (byte) (32 + itemListId);
            packet[9] = (byte) items.Length;
            for(int i = 0; i < items.Length; i++) {
                items[i].ToBytes().CopyTo(packet, 10 + i * 8);
            }
            SendPacket(state, packet);
        }

        public static void SendMenuOptions(ClientState state, bool allowWhispering, bool req1v1Trade, bool allowPartyRequest, bool allowInspection) {
            bool[] bits = new bool[] {
                false, false, !allowInspection, false, !allowPartyRequest, false, !req1v1Trade, !allowWhispering
            };
            byte b = bits.ToByte();

            SendPacket(state, new byte[] { 8, 0, 192, 0, 0, 0, 0, 0, 2, b });
        }

        public static void SendWinScore(ClientState state, int score) {
            byte[] packet = new byte[13];
            packet[0] = 11;
            packet[2] = 192;
            packet[8] = 144;
            BitConverter.GetBytes(score).CopyTo(packet, 9);
            SendPacket(state, packet);
        }
        public static void SendLostScore(ClientState state, int score) {
            byte[] packet = new byte[13];
            packet[0] = 11;
            packet[2] = 192;
            packet[8] = 145;
            BitConverter.GetBytes(score).CopyTo(packet, 9);
            SendPacket(state, packet);
        }
        public static void SendPointScore(ClientState state, int score) {
            byte[] packet = new byte[13];
            packet[0] = 11;
            packet[2] = 192;
            packet[8] = 146;
            BitConverter.GetBytes(score).CopyTo(packet, 9);
            SendPacket(state, packet);
        }

        public static void SendPacket(ClientState client, byte[] packet, bool log = true) {
            if (log) Console.WriteLine("[World] server -> client [id={0}]: (length={1}) {2}", client.Id, packet.Length, Util.ReadableBytes(packet));
            client.ClientSocket.BeginSend(packet, 0, packet.Length, 0, null, client);
        }

        public static void SendPacketToAll(ClientState exception, byte[] packet, bool log = true) {
            foreach(ClientState client in WorldServer.ClientList) {
                if (client != exception) SendPacket(client, packet, log);
            }
        }

        public static void SendPacketToAllOnMap(Map map, ClientState exception, byte[] packet, bool log = true) {
            foreach (ClientState client in WorldServer.ClientList.Where(o => o.PlayerData.MapId == map.Id)) {
                if (client != exception) SendPacket(client, packet, log);
            }
        }
    }
}
