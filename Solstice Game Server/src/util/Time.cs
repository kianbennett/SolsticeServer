using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class Time {

        public static float DeltaTime;

        private static long oldTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        public static void Update() {
            long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            DeltaTime = (time - oldTime) / 1000f;
            oldTime = time;
        }
    }
}
