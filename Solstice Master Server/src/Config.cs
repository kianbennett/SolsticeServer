using Hjson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeMasterServer {

    public class Config {

        public static int Port;
        public static int MaxQueueSize;
        public static Dictionary<string, string> GameServers; // key = name, value = ip

        public static void DefaultValues() {
            Port = 1818;
            MaxQueueSize = 100;
            GameServers = new Dictionary<string, string>();
        }

        public static void LoadValues(string filePath) {
            DefaultValues();

            JsonObject json = null;
            try {
                json = HjsonValue.Load(filePath).Qo();

                Port = json["port"];
                MaxQueueSize = json["max_queue_size"];
                foreach (JsonObject server in json["game_servers"].Qa()) {
                    string name = server["name"];
                    if(name.Length > 13) {
                        string nameShort = name.Substring(0, 13);
                        Console.WriteLine("Server name \"{0}\" exceeds the max length of {1}, shortening to \"{2}\"", name, 13, nameShort);
                        name = nameShort;
                    }
                    GameServers.Add(name, server["ip"]);
                }
            } catch (FileNotFoundException) {
                Console.WriteLine("Missing config file: {0} - using default values", filePath);
                return;
            } catch (Exception) {
                return;
            }
        }
    }
}