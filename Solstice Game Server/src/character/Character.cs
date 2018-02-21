using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class Character {

        public int Id;
        public string Name;
        public int Sex; // 0 = female, 1 = male
        public int HairStyle;
        public int HairColour;

        public byte[] ToBytes() {
            byte[] bytes = new byte[43];

            bytes[0] = (byte) Id;
            Encoding.ASCII.GetBytes(Name).Take(13).ToArray().CopyTo(bytes, 1);
            IPAddress.Parse(Config.IpWorld).GetAddressBytes().Take(4).ToArray().CopyTo(bytes, 15);
            bytes[25] = (byte) (Sex * 128);
            bytes[28] = (byte) (10 * HairStyle + HairColour);

            return bytes;
        }

        public static Character FromBytes(byte[] bytes) {
            Character character = new Character() {
                Id = bytes[0],
                Name = Encoding.ASCII.GetString(bytes.Skip(1).Take(13).ToArray()).Trim((char) 0),
                Sex = bytes[25] > 0 ? 1 : 0,
                HairStyle = (int) Math.Floor(bytes[28] / 10f),
                HairColour = bytes[28] % 10
            };

            return character;
        }
    }
}
