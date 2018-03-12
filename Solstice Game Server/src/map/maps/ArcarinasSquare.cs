using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class ArcarinasSquare : Map {

        public ArcarinasSquare(short id) : base(id) {
            MapExits.Add(new MapExit(86, new Rect(59, 0, 67, 2), 63, 105));

            NpcList.Add(new NpcObject(0, "Transporter", 61456, 6, 85, 46, 2, Id));
            NpcList.Add(new NpcObject(1, "North Gate Guard", 61458, 1, 58, 10, 2, Id));
            NpcList.Add(new NpcObject(2, "North Gate Guard", 61458, 1, 67, 10, 2, Id));

            MonsterList.Add(new MonsterObject(0, 1, 68, 36, 2, Id));

            PetList.Add(new PetObject(0, 1, 101, 6, 2, Id));
            PetList.Add(new PetObject(1, 2, 104, 6, 2, Id));
            PetList.Add(new PetObject(2, 3, 107, 6, 2, Id));
            PetList.Add(new PetObject(3, 4, 110, 6, 2, Id));
            PetList.Add(new PetObject(4, 5, 113, 6, 2, Id));
            PetList.Add(new PetObject(5, 6, 116, 9, 3, Id));
            PetList.Add(new PetObject(6, 7, 116, 12, 3, Id));
            PetList.Add(new PetObject(7, 8, 116, 15, 3, Id));
            PetList.Add(new PetObject(8, 9, 116, 18, 3, Id));
            PetList.Add(new PetObject(9, 10, 116, 21, 3, Id));
        }
    }
}
