using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {

    public class Util {

        private static short lastClientId, lastMapObjectId;

        public static short UniqueClientId() {
            return lastClientId++;
        }
        public static short UniqueMapObjectId() {
            return lastMapObjectId++;
        }

        public static string ReadableBytes(byte[] bytes) {
            string str = "{ ";
            foreach (byte b in bytes) {
                str += b + " ";
            }
            str += "}";
            return str;
        }

        public static string GetSocketAddress(Socket socket) {
            return ((IPEndPoint) socket.RemoteEndPoint).Address.ToString();
        }

        public static T[] FillArray<T>(T[] array, T value) {
            for (int i = 0; i < array.Length; i++) {
                array[i] = value;
            }
            return array;
        }

        public static byte[] EmptyPacket(short length) {
            byte[] packet = new byte[length + 2];
            BitConverter.GetBytes(length).CopyTo(packet, 0);
            return packet;
        }

        public static byte[] PacketWithId(short length, byte id) {
            if(length < 3) {
                Console.WriteLine("[ERROR] Packet length too short");
                return new byte[0];
            }
            byte[] packet = EmptyPacket(length);
            packet[2] = id;
            return packet;
        }

        public static byte[] CombinePackets(params byte[][] packets) {
            List<byte> packet = new List<byte>();
            foreach(byte[] p in packets) {
                packet.AddRange(p);
            }
            return packet.ToArray();
        }
    }

    //** Helper classes **//

    public class Vector2s {
        public short x, y;
        public Vector2s(short x, short y) { this.x = x; this.y = y; }
        public override string ToString() { return "[" + x + ", " + y + "]"; }
    }

    public class Vector2f {
        public float x, y;
        public Vector2f(float x, float y) { this.x = x; this.y = y; }
        public override string ToString() { return "[" + x + ", " + y + "]";}
    }

    public class Rect {
        public Vector2s Min, Max;
        public Rect(short minX, short minY, short maxX, short maxY) {
            Min = new Vector2s(minX, minY);
            Max = new Vector2s(maxX, maxY);
        }

        public bool Contains(Vector2f point) {
            return point.x > Min.x && point.x < Max.x && point.y > Min.y && point.y < Max.y;
        }
    }
}
