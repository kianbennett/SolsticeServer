using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class PlayerObject : MapObject {

        public PlayerData PlayerData;
        public PetObject ActivePet;

        public PlayerObject(ClientState client, short id, PlayerData playerData) : base(id, playerData.PosX, playerData.PosY, playerData.Direction, playerData.MapId) {
            Owner = client;
            PlayerData = playerData;
            Type = ObjectType.Player;
        }

        public override void Update() {
            base.Update();

            foreach(MapExit mapExit in World.MapList[MapId].MapExits) {
                if(mapExit.Rect.Contains(LastMovePos.GetEstimatedPos(PlayerData.MoveSpeed))) {
                    SetPosition(mapExit.TargetPos.x, mapExit.TargetPos.y);
                    Owner.LoadMap(mapExit.MapId);
                }
            }

            //Console.WriteLine(LastMovePos.GetEstimatedPos(PlayerData.MoveSpeed).ToString());
        }

        public byte[] SpawnPlayerPacket(bool playAnim, bool self) {
            bool[] b12 = new bool[8];
            b12[0] = PlayerData.Sex > 0;
            PlayerData.Class.ToBoolArray(true).Skip(1).ToArray().CopyTo(b12, 1);

            byte[] packet = Util.PacketWithId((short) (39 + PlayerData.Name.Length), 177);
            packet[8] = (byte) Type;
            BitConverter.GetBytes(playAnim).CopyTo(packet, 9);
            BitConverter.GetBytes(self ? (short) 0 : Id).CopyTo(packet, 10);
            packet[12] = b12.ToByte(); // Contains both sex and class
            packet[13] = 0; // WepType
            packet[14] = PlayerData.EquipmentOutfit;
            packet[15] = PlayerData.Hair;
            packet[16] = PlayerData.EquipmentHead;
            packet[17] = PlayerData.EquipmentFace;
            packet[18] = PlayerData.EquipmentArmL;
            packet[19] = PlayerData.EquipmentArmR;
            packet[20] = PlayerData.EquipmentEyes;
            packet[21] = PlayerData.EquipmentBack;
            packet[22] = 0;
            packet[23] = PlayerData.MoveSpeed;
            packet[24] = Direction;
            packet[25] = 0; // Stance
            packet[26] = 0;
            BitConverter.GetBytes(PosX).CopyTo(packet, 27);
            BitConverter.GetBytes(PosY).CopyTo(packet, 29);
            packet[31] = 2; // 31 - 35 is guild info
            packet[32] = 3;
            packet[33] = 4;
            packet[34] = 5;
            packet[35] = 6;
            packet[36] = 0;
            packet[37] = 0;
            packet[38] = 0;
            BitConverter.GetBytes((short) PlayerData.Name.Length).CopyTo(packet, 39);
            Encoding.ASCII.GetBytes(PlayerData.Name).CopyTo(packet, 41);
            return packet;
        }

        public byte[] SetPlayerIdPacket() {
            byte[] packet = Util.PacketWithId(13, 191);
            packet[8] = (byte) Type;
            BitConverter.GetBytes((short) 0).CopyTo(packet, 9); // for some reason only an id of 0 will work
            return packet;
        }

        public void SetDirection(byte dir) {
            Direction = dir;
            UpdateClient();
            byte[] packet = Util.PacketWithId(11, 181);
            packet[8] = 18;
            packet[9] = (byte) Type;
            BitConverter.GetBytes(Id).CopyTo(packet, 10);
            packet[12] = dir;

            WorldPacketHandler.SendPacketToAllOnMap(World.MapList[MapId], Owner, packet);
            BitConverter.GetBytes((short) 0).CopyTo(packet, 10);
            WorldPacketHandler.SendPacket(Owner, packet);
        }

        public void SetStance(byte stance) {
            Stance = stance;
            UpdateClient();
            byte[] packet = Util.PacketWithId(12, 181);
            packet[8] = 176;
            packet[9] = (byte) Type;
            BitConverter.GetBytes(Owner.PlayerObject.Id).CopyTo(packet, 10);
            packet[12] = stance;
            packet[13] = Direction;

            WorldPacketHandler.SendPacketToAllOnMap(World.MapList[MapId], Owner, packet);
            BitConverter.GetBytes((short) 0).CopyTo(packet, 10);
            WorldPacketHandler.SendPacket(Owner, packet);
        }

        public void ShowEmoticon(byte emoteId) {
            byte[] packet = Util.PacketWithId(11, 181);
            packet[8] = 22;
            packet[9] = (byte) Type;
            BitConverter.GetBytes(Id).CopyTo(packet, 10);
            packet[12] = emoteId;

            WorldPacketHandler.SendPacketToAllOnMap(World.MapList[MapId], Owner, packet);
            BitConverter.GetBytes((short) 0).CopyTo(packet, 10);
            WorldPacketHandler.SendPacket(Owner, packet);
        }

        public void LevelUp() {
            if(PlayerData.Level == 255) {
                ChatMessagePacketHandler.SendSystemMessage(Owner, "You are at max level!");
                return;
            }
            SetLevel((byte) (PlayerData.Level + 1), true);
        }

        public void SetLevel(byte level, bool effect) {
            PlayerData.Level = level;
            UpdateClient();

            byte[] packet = Util.PacketWithId(8, 192);
            packet[8] = 26;
            packet[9] = PlayerData.Level;
            WorldPacketHandler.SendPacket(Owner, packet);

            if (!effect) return;

            packet = Util.PacketWithId(11, 181);
            packet[8] = 7;
            packet[9] = (byte) Type;
            BitConverter.GetBytes(Id).CopyTo(packet, 10);
            packet[12] = PlayerData.Level;

            WorldPacketHandler.SendPacketToAllOnMap(World.MapList[MapId], Owner, packet);
            BitConverter.GetBytes((short) 0).CopyTo(packet, 10);
            WorldPacketHandler.SendPacket(Owner, packet);
        }

        public void AddItem(Item item) {
            byte[] packet = Util.PacketWithId(15, 192);
            packet[8] = 52;
            item.ToBytes().CopyTo(packet, 9);
            WorldPacketHandler.SendPacket(Owner, packet);
        }

        public void SetKron(int kron) {
            PlayerData.Kron = kron;
            UpdateClient();
            byte[] packet = Util.PacketWithId(15, 192);
            packet[8] = 1;
            BitConverter.GetBytes((Int64) kron).CopyTo(packet, 9);
            WorldPacketHandler.SendPacket(Owner, packet);
        }

        public override void UpdateClient() {
            base.UpdateClient();
            if (Owner == null) return;
            Owner.PlayerData = PlayerData;
            Owner.PlayerData.PosX = PosX;
            Owner.PlayerData.PosY = PosY;
            Owner.PlayerData.MapId = MapId;
            Owner.PlayerData.Direction = Direction;
            Owner.PlayerData.Stance = Stance;
        }
    }
}
