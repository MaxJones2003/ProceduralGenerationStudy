using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class Edge
    {
        public int index;
        public Center d0, d1;  // Delaunay edge
        public Corner v0, v1;  // Voronoi edge

        public List<Site> delaunaySites; // Any Delaunay triangle with this edge in its edges

        [HideInInspector] public Vector2f midpoint;  // halfway between v0,v1
        [HideInInspector] public int river;  // volume of water, or 0

 
    }
}