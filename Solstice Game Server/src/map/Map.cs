using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class Map {

        public short Id;
        public List<PlayerObject> PlayerList = new List<PlayerObject>();
        public List<NpcObject> NpcList = new List<NpcObject>();
        public List<MonsterObject> MonsterList = new List<MonsterObject>();
        public List<PetObject> PetList = new List<PetObject>();

        public List<MapObject> MapObjectsToAdd = new List<MapObject>();
        public List<MapObject> MapObjectsToDelete = new List<MapObject>();

        public List<MapExit> MapExits = new List<MapExit>();

        public bool Enabled;

        public Map(short id, bool enabled = true) {
            Id = id;
            Enabled = enabled;
        }

        public virtual void Update() {
            PlayerObject[] players = PlayerList.ToArray();
            foreach(PlayerObject player in players) player.Update();

            if(MapObjectsToDelete.Count > 0) {
                foreach (MapObject obj in MapObjectsToDelete) removeMapObjectFromList(obj);
                MapObjectsToDelete.Clear();
            }
        }

        public void LoadMap(ClientState client) {
            if(client.PlayerObject != null) DeleteMapObject(client.PlayerObject, true);
            client.PlayerObject = new PlayerObject(client, (short) (1 + PlayerList.Count), client.PlayerData); // id 0 is reserved
            PlayerList.Add(client.PlayerObject);
            byte[] packet = Util.CombinePackets(
                client.PlayerObject.SetPlayerIdPacket(),
                SetMapPacket(client.PlayerData.PosX, client.PlayerData.PosY),
                client.PlayerObject.SpawnPlayerPacket(true, true),
                BuildPacket(client),
                LoadMapPacket(),
                client.PlayerData.PlayerDataPacket()
            );
            WorldPacketHandler.SendPacket(client, packet);

            // Spawn player for all other players on the map
            WorldPacketHandler.SendPacketToAllOnMap(this, client, 
                Util.CombinePackets(client.PlayerObject.SpawnPlayerPacket(true, false), client.PlayerData.PlayerDataPacket())
            );
        }

        public virtual byte[] BuildPacket(ClientState client) {
            List<byte> packet = new List<byte>();
            foreach (NpcObject npc in NpcList) packet.AddRange(npc.SpawnNpcPacket(false));
            foreach (MonsterObject monster in MonsterList) packet.AddRange(monster.SpawnMonsterPacket(false));
            foreach (PetObject pet in PetList) packet.AddRange(pet.SpawnPetPacket(false));
            foreach (PlayerObject player in PlayerList) {
                if(player != client.PlayerObject) {
                    packet.AddRange(player.SpawnPlayerPacket(false, false));
                    packet.AddRange(player.PlayerData.PlayerDataPacket());
                }
            }
            return packet.ToArray();
        }

        /* Example: { 15, 0, 176, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 0, 0, 0 } */
        public byte[] SetMapPacket(short cameraX, short cameraY) {
            byte[] packet = Util.PacketWithId(15, 176);
            packet[8] = 17;
            BitConverter.GetBytes(Id).CopyTo(packet, 9);
            BitConverter.GetBytes(cameraX).CopyTo(packet, 13);
            BitConverter.GetBytes(cameraY).CopyTo(packet, 15);
            return packet;
        }

        public byte[] LoadMapPacket() {
            byte[] packet = Util.PacketWithId(7, 176);
            packet[8] = 19;
            return packet;
        }

        public MapObject GetMapObject(ObjectType type, short id) {
            switch(type) {
                case ObjectType.Player:
                    PlayerObject[] players = PlayerList.Where(o => o.Id == id).ToArray();
                    return players.Length > 0 ? players[0] : null;
                case ObjectType.Monster:
                    MonsterObject[] monsters = MonsterList.Where(o => o.Id == id).ToArray();
                    return monsters.Length > 0 ? monsters[0] : null;
                case ObjectType.Npc:
                    NpcObject[] npcs = NpcList.Where(o => o.Id == id).ToArray();
                    return npcs.Length > 0 ? npcs[0] : null;
                case ObjectType.Pet:
                    PetObject[] pets = PetList.Where(o => o.Id == id).ToArray();
                    return pets.Length > 0 ? pets[0] : null;
            }
            return null;
        }

        public void DeleteMapObject(MapObject mapObject, bool playAnim) {
            MapObjectsToDelete.Add(mapObject);
            mapObject.DeleteObject(playAnim);
        }

        private void removeMapObjectFromList(MapObject mapObject) {
            switch (mapObject.Type) {
                case ObjectType.Player:
                    if (PlayerList.Contains(mapObject)) PlayerList.Remove((PlayerObject) mapObject);
                    break;
                case ObjectType.Monster:
                    if (MonsterList.Contains(mapObject)) MonsterList.Remove((MonsterObject) mapObject);
                    break;
                case ObjectType.Npc:
                    if (NpcList.Contains(mapObject)) NpcList.Remove((NpcObject) mapObject);
                    break;
                case ObjectType.Pet:
                    if (PetList.Contains(mapObject)) PetList.Remove((PetObject) mapObject);
                    break;
            }
        }
    }
}
