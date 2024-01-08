using UnityEngine;

namespace Map
{
    public class Edge
    {
        public int index;
        [HideInInspector]public Center d0, d1;  // Delaunay edge
        [HideInInspector]public Corner v0, v1;  // Voronoi edge

        [HideInInspector] public Vector2f midpoint;  // halfway between v0,v1
        [HideInInspector] public int river;  // volume of water, or 0
    }
}