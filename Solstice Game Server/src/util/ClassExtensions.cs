using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {

    public static class ByteExtensions {

        /* Converts a byte to its binary value */
        public static bool[] ToBoolArray(this byte i, bool padLeft = false) {
            string str = Convert.ToString(i, 2);
            if (padLeft) str = str.PadLeft(8, '0');
            return str.Select(s => s.Equals('1')).ToArray();
        }
    }

    public static class BoolArrayExtensions {

        /* Converts the first 8 values of a bool array to a byte */
        public static byte ToByte(this bool[] bools) {
            Array.Reverse(bools);
            BitArray bits = new BitArray(bools);
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }
    }
}