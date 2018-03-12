using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolsticeGameServer {
    public class Path {

        public class Node {
            public short x, y;

            public Node(short x, short y) {
                this.x = x;
                this.y = y;
            }

            public float Dist(Node other) {
                float x = other.x - this.x;
                float y = other.y - this.y;
                return (float) Math.Sqrt(x * x + y * y);
            }

            public override string ToString() {
                return "[" + x + ", " + y + "]";
            }
        }

        public List<Node> Nodes;
        public float TotalDist;

        public Path(short startX, short startY, short endX, short endY) {
            Node nodeStart = new Node(startX, startY);
            Node nodeEnd = new Node(endX, endY);

            Nodes = new List<Node> { nodeStart };

            // If already lined up horizontally or diagonally
            if (!(startX == endX || startY == endY || (endX - startX) == (endY - startY))) {
                // Direction of diagon (from end point)
                int xDir = startX < endX ? -1 : 1;
                int yDir = startY < endY ? -1 : 1;

                // Second point on line to get y = mx + c
                int x2 = endX + xDir;
                int y2 = endY + yDir;

                float m = (float) yDir / xDir;
                float c = endY - m * endX;

                // The two points on the line that are horizontally in line with the start point
                Node vert = new Node(startX, (short) (m * startX + c));
                Node hori = new Node((short) ((startY - c) / m), startY);

                // Add whichever is closer to the end
                Nodes.Add(vert.Dist(nodeEnd) > hori.Dist(nodeEnd) ? hori : vert);
            }

            Nodes.Add(nodeEnd);

            for (int i = 0; i < Nodes.Count; i++) {
                if (i == 0) continue;
                TotalDist += Nodes[i].Dist(Nodes[i - 1]);
            }
        }

        public Vector2f GetPoint(float distTravelled) {
            if(distTravelled > TotalDist) {
                return new Vector2f(Nodes[Nodes.Count - 1].x, Nodes[Nodes.Count - 1].y);
            }

            Node nodePrev = Nodes[0];
            Node nodeNext = Nodes[1];
            float dist = 0;
            float remainder = 0;

            for(int i = 0; i < Nodes.Count; i++) {
                if (i == Nodes.Count - 1) break;

                nodePrev = Nodes[i];
                nodeNext = Nodes[i + 1];

                remainder = distTravelled - dist;
                dist += nodeNext.Dist(nodePrev);
                if (dist > distTravelled) break;
            }

            float d = nodeNext.Dist(nodePrev);
            float t = remainder / d;
            if (d == 0) t = 0;

            Vector2f point = new Vector2f((1 - t) * nodePrev.x + t * nodeNext.x, (1 - t) * nodePrev.y + t * nodeNext.y);

            return point;
        }
    }
}
