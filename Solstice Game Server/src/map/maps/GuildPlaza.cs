using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class GuildPlaza : Map {

        public GuildPlaza(short id) : base(id) {
            MapExits.Add(new MapExit(84, new Rect(58, 121, 67, 125), 63, 7));
        }
    }
}