using csDelaunay;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class Corner : IKDTreeItem
    {
        public int index;

        [HideInInspector]public List<Site> sites; // Any Voronoi cell with this point in its edges

        public Vector2f point;  // location
        #region IKDTreeItem
        public Vector2f Position => point;

        public float this[int dimension] => point[dimension];
        #endregion

        public bool ocean;  // ocean
        public bool water;  // lake or ocean
        public bool coast;  // touches ocean and land polygons
        public bool border;  // at the edge of the island
        public float elevation;  // 0.0-1.0
        public float moisture;  // 0.0-1.0

        [HideInInspector] public List<Center> touches;
        [HideInInspector] public List<Edge> protrudes;
        [HideInInspector] public List<Corner> adjacent;


        [HideInInspector] public int? river;  // 0 if no river, or volume of water in river
        [HideInInspector] public Corner downslope;  // pointer to adjacent corner most downhill
        [HideInInspector] public Corner watershed;  // pointer to coastal corner, or null
        [HideInInspector] public int? watershed_size;
        private Vector2f p0;


        #region Overrides
        // Override comparison for a Vector2f and a Corner, where the Vector2f is the point of the corner
        public static bool operator ==(Corner c, Vector2f v)
        {
            return c.point == v;
        }
        // Define the != operator as well
        public static bool operator !=(Corner c, Vector2f v)
        {
            return !(c == v);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
        #endregion
}