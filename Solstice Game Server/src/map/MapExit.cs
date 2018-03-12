using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class MapExit {

        public short MapId;
        public Rect Rect;
        public Vector2s TargetPos;

        public MapExit(short mapId, Rect rect, short posX, short posY) {
            MapId = mapId;
            Rect = rect;
            TargetPos = new Vector2s(posX, posY);
        }
    }
}
