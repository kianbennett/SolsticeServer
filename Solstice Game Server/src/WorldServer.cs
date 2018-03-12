using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SolsticeGameServer {

    /* Handles in-game data */
    public class WorldServer {

        public static ManualResetEvent ResetEvent = new ManualResetEvent(false);
        public static List<ClientState> ClientList = new List<ClientState>();

        private const int tickRate = 30;

        public static void StartListening() {
            World.Init();
            new Thread(new ThreadStart(updateThread)).Start();

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Config.PortWorld);

            Console.WriteLine("[World] Starting world server on port " + localEndPoint.Port);

            // Create a TCP socket
            Socket listener = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("[World] Started socket, waiting for connections...");

            try {
                listener.Bind(localEndPoint);
                listener.Listen(Config.MaxQueueSizeWorld);

                while (true) {
                    // Set the event to nonsignaled state
                    ResetEvent.Reset();

                    // Start an asynchronous socket to listen for connections
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing
                    ResetEvent.WaitOne();
                }
            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public static void AcceptCallback(IAsyncResult result) {
            // Signal the main thread to continue
            ResetEvent.Set();

            // Get the socket that handles the client request
            Socket listener = (Socket) result.AsyncState;
            Socket handler = listener.EndAccept(result);

            int id = Util.UniqueClientId();

            Console.WriteLine("[World] Accepted new connection from {0} [id={1}]", Util.GetSocketAddress(handler), id);

            // Create the state object.  
            ClientState state = new ClientState(id, handler, new AsyncCallback(ReceiveCallback), true);
            ClientList.Add(state);
        }

        public static void ReceiveCallback(IAsyncResult ar) {
            // Retrieve the state object and the handler socket from the asynchronous state object
            ClientState state = (ClientState) ar.AsyncState;
            state.ResetEvent.Set();
            Socket socket = state.ClientSocket;

            int totalBytes = socket.EndReceive(ar);
            if (totalBytes < 2) return;

            int bytesRead = 0;

            while(bytesRead < totalBytes) {
                short length = BitConverter.ToInt16(state.Buffer, bytesRead);
                byte[] packet = new byte[length + 2];
                Array.Copy(state.Buffer, bytesRead, packet, 0, length + 2);
                WorldPacketHandler.RecievePacket(state, packet);
                bytesRead += length + 2;
            }

            byte[] data = new byte[bytesRead];
            Array.Copy(state.Buffer, data, data.Length);

            short packetLength = BitConverter.ToInt16(data, 0);
        }

        private static void updateThread() {
            while (true) {
                Time.Update();
                World.Update();
                Thread.Sleep((int) (1000f / tickRate));
            }
        }
    }
}
