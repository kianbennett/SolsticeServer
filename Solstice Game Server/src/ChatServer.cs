using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SolsticeGameServer {

    public class ChatServer {

        public static ManualResetEvent ResetEvent = new ManualResetEvent(false);
        public static List<ClientState> ClientList = new List<ClientState>();

        private static int connectionCount;

        public static void StartListening() {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Config.PortChat);

            Console.WriteLine("[Chat] Starting chat server on port " + localEndPoint.Port);

            Socket listener = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("[Chat] Started socket, waiting for connections...");

            try {
                listener.Bind(localEndPoint);
                listener.Listen(Config.MaxQueueSizeChat);

                while (true) {
                    ResetEvent.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    ResetEvent.WaitOne();
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public static void AcceptCallback(IAsyncResult result) {
            ResetEvent.Set();

            Socket listener = (Socket) result.AsyncState;
            Socket handler = listener.EndAccept(result);

            Console.WriteLine("[Chat] Accepted new connection from {0} [id={1}]", Util.GetSocketAddress(handler), connectionCount);

            ClientState state = new ClientState(connectionCount, handler, new AsyncCallback(ReceiveCallback), false);
            ClientList.Add(state);

            connectionCount++;
        }

        public static void ReceiveCallback(IAsyncResult ar) {
            ClientState state = (ClientState) ar.AsyncState;
            state.ResetEvent.Set();
            Socket socket = state.ClientSocket;

            int bytesRead = socket.EndReceive(ar);
            if (bytesRead == 0) return;
            byte[] data = new byte[bytesRead];
            Array.Copy(state.Buffer, data, data.Length);

            ChatPacketHandler.RecievePacket(state, data);
        }

        public static ClientState GetClientFromName(string name) {
            ClientState[] results = ClientList.Where(o => o.Username == name).ToArray();
            if (results.Length > 0) return results[0];
                else return null;
        }
    }
}
