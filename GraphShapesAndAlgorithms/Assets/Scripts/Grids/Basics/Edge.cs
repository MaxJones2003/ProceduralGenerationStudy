using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    [System.Serializable]
    public class Edge
    {
        public int Q;
        public int R;
        public Direction Direction;
        public bool isGridBounds = false;
        public Edge(Vertex startPoint, Vertex endPoint, Direction direction, int q, int r)
        {
            Q = q;
            R = r;
            endPoints = new Vertex[2];
            endPoints[0] = startPoint;
            endPoints[1] = endPoint;
            Direction = direction;
        }

        /// <summary>
        /// Faces that share this edge.
        /// </summary>
        public Face[] joins { get; set; }
        /// <summary>
        /// Edges that share a vertex with this edge.
        /// </summary>
        public Edge[] continues { get; set; }
        /// <summary>
        ///  The two vertices that make up this edge.
        /// </summary>
        public Vertex[] endPoints { get; set; }
    }

    public enum Direction
    {
        North,
        West
    }
}
