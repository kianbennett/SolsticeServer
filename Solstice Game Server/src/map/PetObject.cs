using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class PetObject : MapObject {

        public short PetId;

        public PetObject(short id, short petId, short posX, short posY, byte dir, short mapId) : base(id, posX, posY, dir, mapId) {
            PetId = petId;
            Type = ObjectType.Pet;
        }

        public byte[] SpawnPetPacket(bool playAnim) {
            byte[] packet = Util.PacketWithId(22, 177);
            packet[8] = (byte) Type;
            BitConverter.GetBytes(playAnim).CopyTo(packet, 9);
            BitConverter.GetBytes(Id).CopyTo(packet, 10);
            BitConverter.GetBytes(PetId).CopyTo(packet, 12);
            packet[14] = 0; // ?
            packet[15] = 20; // move speed
            packet[16] = Direction;
            packet[17] = 2; // stance (2 = alive, 4 = dead)
            BitConverter.GetBytes(PosX).CopyTo(packet, 18);
            BitConverter.GetBytes(PosY).CopyTo(packet, 20);
            packet[22] = 0; // ?
            packet[23] = 0; // ?
            return packet;
        }
    }
}
