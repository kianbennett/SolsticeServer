using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Data.SQLite;

namespace SolsticeGameServer {

    public class Program {

        public static SQLiteConnection SqlConnection;

        // When the program is stopped with ctrl-c
        public static void InterruptHandler(object sender, ConsoleCancelEventArgs args) {
            Console.WriteLine("Shutting down server...");
        }

        public static void Main(string[] args) {
            Config.LoadValues("gameserver.config");
            Console.CancelKeyPress += InterruptHandler;

            SqlConnection = new SQLiteConnection("Data Source=" + Config.DatabaseFile + ";Version=3;");
            SqlConnection.Open();

            Thread svcThread = new Thread(new ThreadStart(SvcServer.StartListening));
            Thread worldThread = new Thread(new ThreadStart(WorldServer.StartListening));

            svcThread.Start();
            worldThread.Start();
        }
    }
}
