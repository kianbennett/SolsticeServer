using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class NpcObject : MapObject {

        public string Name;
        public ushort SpriteId;
        public short DialogId;

        public NpcObject(short id, string name, ushort spriteId, short dialogId, short posX, short posY, byte dir, short mapId) : base(id, posX, posY, dir, mapId) {
            Name = name;
            SpriteId = spriteId;
            DialogId = dialogId;
            Type = ObjectType.Npc;
        }

        public byte[] SpawnNpcPacket(bool playAnim) {
            byte[] packet = Util.PacketWithId((short) (23 + Name.Length), 177);
            packet[8] = (byte) Type;
            BitConverter.GetBytes(playAnim).CopyTo(packet, 9);
            BitConverter.GetBytes(Id).CopyTo(packet, 10);
            BitConverter.GetBytes(SpriteId).CopyTo(packet, 12);
            packet[14] = 0; // spawn effect id
            packet[16] = Direction;
            packet[17] = 1; // can talk to
            BitConverter.GetBytes(DialogId).CopyTo(packet, 18);
            BitConverter.GetBytes(PosX).CopyTo(packet, 20);
            BitConverter.GetBytes(PosY).CopyTo(packet, 22);
            packet[24] = (byte) Name.Length;
            Encoding.ASCII.GetBytes(Name).CopyTo(packet, 25);
            return packet;
        }
    }
}
