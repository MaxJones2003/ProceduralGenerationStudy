using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geometry
{
    [System.Serializable]
    public class Face
    {
        public int Q;
        public int R;
        public bool isOutOfGrid = false;
        /// <summary>
        /// Faces sharing a border with this face.
        /// </summary> 
        
        public Face[] neighbors { get; set; }
        /// <summary>
        /// All edges of this face.
        /// </summary>
        public Edge[] borders { get; set; }
        /// <summary>
        /// All vertices of this face.
        /// </summary>
        public Vertex[] corners { get; set; }
        public Face(int q, int r, Vertex[] vertices)
        {
            Q = q;
            R = r;
            corners = vertices;
        }
    }
}

