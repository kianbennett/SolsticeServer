using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {

    public enum Skill {
        POW, INT, STA, AGI, MEN, WIS
    }

    public class PlayerData {

        public byte LocalId;

        // Appearance
        public string Name;
        public byte Sex; // 0 = female, 1 = male
        public byte Hair;
        public byte Class;

        // Items
        public byte EquipmentOutfit;
        public byte EquipmentHead;
        public byte EquipmentFace;
        public byte EquipmentEyes;
        public byte EquipmentGloves;
        public byte EquipmentBack;
        public byte EquipmentArmL, EquipmentArmR;
        public byte EquipmentShoes;
        public short EquipmentMSlot;
        public byte EquipmentAcc1, EquipmentAcc2, EquipmentAcc3;
        public int Kron;

        // Stats
        public byte Level = 1;
        public byte Power, Stamina, Agility, Intelligence, Mentality, Wisdom;
        public byte MoveSpeed = 20;
        public short Hp, Mp;
        public byte SkillPoints;
        public int PvpWin, PvpLost;
        public byte PvpPoint;

        // Location
        public short MapId;
        public short PosX, PosY;
        public byte Direction;
        public byte Stance;

        // Social
        public List<string> BuddyList;

        public byte[] SvcBytes() {
            byte[] bytes = new byte[43];
            bytes[0] = LocalId;
            Encoding.ASCII.GetBytes(Name).Take(13).ToArray().CopyTo(bytes, 1);
            IPAddress.Parse(Config.IpWorld).GetAddressBytes().Take(4).ToArray().CopyTo(bytes, 15);
            bytes[19] = Power;
            bytes[20] = Stamina;
            bytes[21] = Agility;
            bytes[22] = Intelligence;
            bytes[23] = Mentality;
            bytes[24] = Wisdom;
            bytes[25] = (byte) (Sex * 128);
            bytes[27] = EquipmentOutfit;
            bytes[28] = Hair;
            bytes[29] = EquipmentHead;
            bytes[30] = EquipmentFace;
            bytes[31] = EquipmentArmL;
            bytes[32] = EquipmentArmR;
            bytes[33] = EquipmentEyes;
            bytes[34] = EquipmentBack;
            bytes[35] = 0;      // ?
            bytes[36] = 0;      // ?
            bytes[37] = 0;      // ?
            bytes[38] = 0;      // ?
            bytes[39] = 0;      // ?
            bytes[40] = 0;      // ?
            bytes[41] = 0;      // ?
            bytes[42] = 0;      // ?
            return bytes;
        }

        public static PlayerData FromBytes(byte[] bytes) {
            PlayerData data = new PlayerData() {
                LocalId = bytes[0],
                Name = Encoding.ASCII.GetString(bytes.Skip(1).Take(13).ToArray()).Trim((char) 0),
                Power = bytes[19],
                Stamina = bytes[20],
                Agility = bytes[21],
                Intelligence = bytes[22],
                Mentality = bytes[23],
                Wisdom = bytes[24],
                Sex = (byte) (bytes[25] > 0 ? 1 : 0),
                EquipmentOutfit = bytes[27],
                Hair = bytes[28],
                EquipmentHead = bytes[29],
                EquipmentFace = bytes[30],
                EquipmentEyes = bytes[33],
                EquipmentBack = bytes[34],
            };
            return data;
        }

        public byte[] PlayerDataPacket() {
            byte[] packet = Util.PacketWithId(181, 192);
            packet[8] = 0;
            BitConverter.GetBytes(Kron).CopyTo(packet, 9);
            packet[17] = 0; // menu options
            packet[18] = Class;
            packet[19] = 0; // ?
            BitConverter.GetBytes(Hp).CopyTo(packet, 20);
            BitConverter.GetBytes(Mp).CopyTo(packet, 22);
            packet[24] = Level;
            packet[25] = 0; // ?
            packet[26] = 0; // ?
            packet[27] = SkillPoints;
            packet[28] = 0; // ?
            packet[29] = Power;
            packet[30] = Intelligence;
            packet[31] = Stamina;
            packet[32] = Agility;
            packet[33] = Mentality;
            packet[34] = Wisdom;
            BitConverter.GetBytes(PvpWin).CopyTo(packet, 35);
            BitConverter.GetBytes(PvpLost).CopyTo(packet, 39);
            packet[43] = PvpPoint;
            packet[44] = 0; // ?
            packet[45] = 0; // ?
            packet[46] = 0; // ?
            packet[47] = 0; // ?
            packet[48] = 0; // ?
            packet[49] = 0; // ?
            packet[50] = 0; // ?
            packet[51] = 0; // ?
            packet[52] = 0; // ?
            BitConverter.GetBytes(EquipmentMSlot).CopyTo(packet, 53);
            BitConverter.GetBytes(EquipmentEyes).CopyTo(packet, 71); // for now don't include expiry info
            BitConverter.GetBytes(EquipmentFace).CopyTo(packet, 79);
            BitConverter.GetBytes(EquipmentArmL).CopyTo(packet, 87);
            BitConverter.GetBytes(EquipmentArmR).CopyTo(packet, 95);
            BitConverter.GetBytes(EquipmentBack).CopyTo(packet, 103);
            BitConverter.GetBytes(EquipmentGloves).CopyTo(packet, 111);
            BitConverter.GetBytes(EquipmentShoes).CopyTo(packet, 119);
            BitConverter.GetBytes(EquipmentAcc1).CopyTo(packet, 127);
            BitConverter.GetBytes(EquipmentAcc2).CopyTo(packet, 135);
            BitConverter.GetBytes(EquipmentAcc3).CopyTo(packet, 133);
            BitConverter.GetBytes(EquipmentOutfit).CopyTo(packet, 141);
            BitConverter.GetBytes(EquipmentHead).CopyTo(packet, 149);
            // bytes 157+ are ?
            return packet;
        }

        public void AddSkillPoint(ClientState client, Skill skill) {
            if (SkillPoints <= 0) return;
            byte[] packet1 = Util.PacketWithId(9, 192);
            switch (skill) {
                case Skill.POW:
                    packet1[8] = 33;
                    packet1[9] = ++Power;
                    break;
                case Skill.INT:
                    packet1[8] = 34;
                    packet1[9] = ++Intelligence;
                    break;
                case Skill.STA:
                    packet1[8] = 35;
                    packet1[9] = ++Stamina;
                    break;
                case Skill.AGI:
                    packet1[8] = 36;
                    packet1[9] = ++Agility;
                    break;
                case Skill.MEN:
                    packet1[8] = 37;
                    packet1[9] = ++Mentality;
                    break;
                case Skill.WIS:
                    packet1[8] = 38;
                    packet1[9] = ++Wisdom;
                    break;
            }
            byte[] packet2 = Util.PacketWithId(9, 192);
            packet2[8] = 47;
            packet2[9] = --SkillPoints;
            WorldPacketHandler.SendPacket(client, Util.CombinePackets(packet1, packet2));
        }
    }
}
