using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class MonsterObject : MapObject {

        public ushort MonsterId;

        public MonsterObject(short id, ushort monsterId, short posX, short posY, byte dir, short mapId) : base(id, posX, posY, dir, mapId) {
            MonsterId = monsterId;
            Type = ObjectType.Monster;
        }

        public byte[] SpawnMonsterPacket(bool playAnim) {
            byte[] packet = Util.PacketWithId(24 + 0, 177);
            packet[8] = (byte) Type;
            BitConverter.GetBytes(playAnim).CopyTo(packet, 9);
            BitConverter.GetBytes(Id).CopyTo(packet, 10);
            BitConverter.GetBytes(MonsterId).CopyTo(packet, 12);
            packet[14] = 1; // can attack
            packet[15] = 20; // move speed
            packet[16] = Direction;
            packet[17] = 2; // stance (2 = combat, 3 = dead)
            packet[18] = 20; // health percentage (0-20 -> 0 = 0%, 10 = 50%, 20 = 100%)
            BitConverter.GetBytes(PosX).CopyTo(packet, 19);
            BitConverter.GetBytes(PosY).CopyTo(packet, 21);
            packet[23] = 0; // ?
            packet[24] = 0; // ?
            packet[25] = 0; // number of visual effects (26+ are the effects)
            return packet;
        }
    }
}
