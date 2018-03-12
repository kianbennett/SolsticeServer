using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SolsticeGameServer {

    // State object for reading client data asynchronously  
    public class ClientState {

        public int Id;
        public string Username;
        public Socket ClientSocket = null;
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];

        public ManualResetEvent ResetEvent;
        private Thread threadRecv, threadCheckAlive;
        private bool active;
        public bool IsAlive;
        private AsyncCallback receiveCallback;

        public PlayerData PlayerData;
        public PlayerObject PlayerObject;

        public ClientState(int id, Socket socket, AsyncCallback receiveCallback, bool checkAlive) {
            Id = id;
            ClientSocket = socket;
            this.receiveCallback = receiveCallback;

            ResetEvent = new ManualResetEvent(false);
            threadRecv = new Thread(new ThreadStart(checkForIncomingData));
            threadRecv.Start();
            if(checkAlive) {
                threadCheckAlive = new Thread(new ThreadStart(checkIsAlive));
                threadCheckAlive.Start();
            }
        }

        private void checkForIncomingData() {
            active = true;
            while (active) {
                ResetEvent.Reset();
                ClientSocket.BeginReceive(Buffer, 0, BufferSize, 0, receiveCallback, this);
                ResetEvent.WaitOne();
            }

            Console.WriteLine("Closed connection for {0} [id={1}]", Util.GetSocketAddress(ClientSocket), Id);

            if (WorldServer.ClientList.Contains(this)) WorldServer.ClientList.Remove(this);
            if (PlayerObject != null) {
                Map map = World.MapList[PlayerData.MapId];
                map.DeleteMapObject(PlayerObject, false);
            }
        }

        private void checkIsAlive() {
            while(active) { // If keep-alive packet has not been received within timeout period, close the client (packet is sent every 5 seconds)
                Thread.Sleep(Config.WorldTimeout * 1000);
                if (!IsAlive) Close();
                IsAlive = false;
            }
        }

        public void LoadMap(short mapId) {
            if (PlayerObject != null) World.MapList[PlayerObject.MapId].DeleteMapObject(PlayerObject, true);
            PlayerData.MapId = mapId;
            World.MapList[mapId].LoadMap(this);
        }

        public void Close() {
            active = false;
        }
    }
}
