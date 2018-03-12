using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    
    public enum ObjectType { Player = 1, Monster = 2, Npc = 3, Item = 4, Deal = 5, Buff = 6, Pet = 7 }

    public class MapObject {

        public class MovePos {
            public short TargetX, TargetY;
            public short OriginX, OriginY;
            public long TimeStamp;
            public Path Path;

            public MovePos(short targetX, short targetY, short originX, short originY) {
                TargetX = targetX;
                TargetY = targetY;
                OriginX = originX;
                OriginY = originY;
                TimeStamp = DateTimeOffset.Now.Millisecond;
                Path = new Path(originX, originY, targetX, targetY);

                //Console.WriteLine(TargetX + ", " + TargetY + ", " + OriginX + ", " + OriginY);
            }

            public Vector2f GetEstimatedPos(float moveSpeed) {
                long time = DateTimeOffset.Now.Millisecond;
                float dist = 0.0075f * (time - TimeStamp);
                //Console.WriteLine(dist);
                return Path.GetPoint(dist);
            }
        }

        public ClientState Owner;

        public short Id;
        public short MapId;
        public short PosX, PosY;
        public byte Direction, Stance;
        public ObjectType Type;

        public MovePos LastMovePos;

        public MapObject(short id, short posX, short posY, byte dir, short mapId) {
            Id = id;
            PosX = posX;
            PosY = posY;
            Direction = dir;
            MapId = mapId;
            LastMovePos = new MovePos(posX, posY, posX, posY);
            UpdateClient();
        }

        public virtual void Update() {
        }

        public byte[] DamageTargetPacket(byte targetType, short targetId, params short[] dmg) {
            byte[] packet = Util.PacketWithId((short) (18 + dmg.Length * 2), 181);
            packet[8] = 177;
            packet[9] = targetType;
            BitConverter.GetBytes(targetId).CopyTo(packet, 10);
            packet[12] = (byte) Type;
            BitConverter.GetBytes(Id).CopyTo(packet, 13);
            packet[15] = 0; // ?
            packet[16] = 1; // attack speed
            packet[17] = (byte) dmg.Length; // first wave count
            packet[18] = 0; // second wave count
            packet[19] = 0; // ?
            for(int i = 0; i < dmg.Length; i++) {
                BitConverter.GetBytes(dmg[i]).CopyTo(packet, 20 + i * 2);
            }
            //BitConverter.GetBytes((short) 4).CopyTo(packet, 22); // 2nd hit
            //BitConverter.GetBytes((short) 5).CopyTo(packet, 24); // 3rd hit
            //BitConverter.GetBytes((short) 6).CopyTo(packet, 26); // 4th hit
            return packet;
        }

        public void DeleteObject(bool effect) {
            byte[] packet = Util.PacketWithId(10, 180);
            packet[8] = (byte) Type;
            packet[9] = BitConverter.GetBytes(effect)[0];
            BitConverter.GetBytes(Id).CopyTo(packet, 10);

            WorldPacketHandler.SendPacketToAllOnMap(World.MapList[MapId], Owner, packet, false);
            if (Owner != null) {
                packet[9] = packet[10] = 0;
                WorldPacketHandler.SendPacket(Owner, packet, false);
            }
        }

        public byte[] DeleteObjectPacket(bool playAnim) {
            byte[] packet = Util.PacketWithId(10, 180);
            packet[8] = (byte) Type;
            packet[9] = BitConverter.GetBytes(playAnim)[0];
            BitConverter.GetBytes(Id).CopyTo(packet, 10);
            return packet;
        }

        public void SetPosition(short x, short y) {
            if (LastMovePos != null) {
                Vector2f oldPos = LastMovePos.GetEstimatedPos(20); // TODO: change this to actual move speed
                //Console.WriteLine(oldPos);
                LastMovePos = new MovePos(x, y, (short) oldPos.x, (short) oldPos.y);
            } else {
                LastMovePos = new MovePos(x, y, PosX, PosY);
            }

            PosX = x;
            PosY = y;
            UpdateClient();
            byte[] packet = Util.PacketWithId(13, 179);
            packet[8] = (byte) Type;
            BitConverter.GetBytes(Id).CopyTo(packet, 9);
            BitConverter.GetBytes(PosX).CopyTo(packet, 11);
            BitConverter.GetBytes(PosY).CopyTo(packet, 13);

            WorldPacketHandler.SendPacketToAllOnMap(World.MapList[MapId], Owner, packet, false);
            if(Owner != null) {
                packet[9] = packet[10] = 0;
                WorldPacketHandler.SendPacket(Owner, packet, false);
            }
        }

        public static Type GetTypeClass(ObjectType type) {
            switch(type) {
                case ObjectType.Player:
                    return typeof(PlayerObject);
                case ObjectType.Monster:
                    return typeof(MonsterObject);
                case ObjectType.Npc:
                    return typeof(NpcObject);
                case ObjectType.Pet:
                    return typeof(PetObject);
            }
            return null;
        }

        // For every client the player's own id must always be 0
        public short GetId(ClientState client) {
            return (this is PlayerObject && (this as PlayerObject) == client.PlayerObject) ? (short) 0 : Id;
        }

        public virtual void UpdateClient() {
        }
    }
}
