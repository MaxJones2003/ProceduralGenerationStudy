using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class Center
    {
        //public Center(Vector2f point, Site site)
        //{
        //    this.point = point;
        //    sites = new();
        //    sites.Add(site);
        //     neighbors = new List<Center>();
        //    borders = new List<Edge>();
        //    corners = new List<Corner>(); 
        //}

        public int index;

        public Vector2f point;  // location
        public List<Site> sites; // Any Delaunay cell with this point in its edges

        public bool water;  // lake or ocean
        public bool ocean;  // ocean
        public bool coast;  // land polygon touching an ocean
        public bool border;  // at the edge of the map
        public string biome;  // biome type (see article)
        public float elevation;  // 0.0-1.0
        public float moisture;  // 0.0-1.0

        [HideInInspector] public List<Center> neighbors;
        [HideInInspector] public List<Edge> borders;
        [HideInInspector] public List<Corner> corners;

        #region Overrides
        // Override comparison for a Vector2f and a Center, where the Vector2f is the point of the center
        public static bool operator ==(Center c, Vector2f v)
        {
            return c.point == v;
        }

        // Define the != operator as well
        public static bool operator !=(Center c, Vector2f v)
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
        #endregion

    }
}
