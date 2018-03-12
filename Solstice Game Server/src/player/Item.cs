using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class Item {

        public short Id;
        public short Amount;
        public int ExpiryInfo;

        public Item(Int16 id, Int16 amount) {
            Id = id;
            Amount = amount;
        }

        public byte[] ToBytes() {
            byte[] bytes = new byte[8];
            BitConverter.GetBytes(Id).Take(2).ToArray().CopyTo(bytes, 0);
            BitConverter.GetBytes(Amount).Take(2).ToArray().CopyTo(bytes, 2);
            BitConverter.GetBytes(ExpiryInfo).Take(4).ToArray().CopyTo(bytes, 4);
            return bytes;
        }

        public static Item FromBytes(byte[] bytes) {
            return new Item(BitConverter.ToInt16(bytes, 0), BitConverter.ToInt16(bytes, 2)) {
                ExpiryInfo = BitConverter.ToInt32(bytes, 4)
            };
        }
    }
}
