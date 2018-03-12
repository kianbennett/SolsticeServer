//using Microsoft.Data.Sqlite;
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
//using System.Data.SQLite;

namespace SolsticeGameServer {

    public class Program {

        public static SqliteConnection SqlConnection;

        // When the program is stopped with ctrl-c
        public static void InterruptHandler(object sender, ConsoleCancelEventArgs args) {
            Console.WriteLine("Shutting down server...");
        }

        public static void Main(string[] args) {
            Config.LoadValues("gameserver.config");
            Console.CancelKeyPress += InterruptHandler;

            SqlConnection = new SqliteConnection("Data Source=" + Config.DatabaseFile + ";");
            SqlConnection.Open();

            Thread svcThread = new Thread(new ThreadStart(SvcServer.StartListening));
            Thread worldThread = new Thread(new ThreadStart(WorldServer.StartListening));
            Thread chatThread = new Thread(new ThreadStart(ChatServer.StartListening));

            svcThread.Start();
            worldThread.Start();
            chatThread.Start();
        }
    }
}
