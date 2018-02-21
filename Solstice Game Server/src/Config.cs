using Hjson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {

    public class Config {

        public static int PortSvc, PortWorld;
        public static string IpWorld;
        public static int MaxQueueSizeSvc, MaxQueueSizeWorld;
        public static string DatabaseFile;

        public static void DefaultValues() {
            PortSvc = 1819;
            PortWorld = 18123;
            IpWorld = "127.0.0.1";
            MaxQueueSizeSvc = 100;
            MaxQueueSizeWorld = 100;
            DatabaseFile = "characters.db";
        }

        public static void LoadValues(string filePath) {
            DefaultValues();

            JsonObject json = null;
            try {
                json = HjsonValue.Load(filePath).Qo();

                PortSvc = json["svc"]["port"];
                PortWorld = json["world"]["port"];
                IpWorld = json["world"]["ip"];
                MaxQueueSizeSvc = json["svc"]["max_queue_size"];
                MaxQueueSizeWorld = json["world"]["max_queue_size"];
                DatabaseFile = json["database"];
            } catch (FileNotFoundException) {
                Console.WriteLine("Missing config file: {0} - using default values", filePath);
                return;
            } catch (Exception) {
                return;
            }
        }
    }
}