using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    [System.Serializable]
    public class Vertex
    {
        public int Q;
        public int R;
        /// <summary>
        /// Faces that share this vertex.
        /// </summary>
        public Face[] touches { get; set; }
        /// <summary>
        /// Edges that share this vertex.
        /// </summary>
        public Edge[] protrudes { get; set; }
        /// <summary>
        /// Vertecies that are adcacent to this vertex. Any protrudes of this vertex will contain a vertex in this list.
        /// </summary>
        public Vertex[] adjacent { get; set; }

        public Vertex(int q, int r)
        {
            Q = q;
            R = r;
        }
    }
}

