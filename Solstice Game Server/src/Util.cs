using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {

    public class Util {

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
    }
}
