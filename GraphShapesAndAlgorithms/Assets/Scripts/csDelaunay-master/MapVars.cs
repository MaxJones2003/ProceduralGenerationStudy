using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
namespace Map
{
    public class Center
    {
        public Center(Vector2f point, Site site)
        {
            this.point = point;
            sites = new();
            sites.Add(site);
            /* neighbors = new List<Center>();
            borders = new List<Edge>();
            corners = new List<Corner>(); */
        }

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

    }

    public class Corner
    {
        public int index;

        public List<Site> sites; // Any Voronoi cell with this point in its edges
  
        public Vector2f point;  // location
        public bool ocean;  // ocean
        public bool water;  // lake or ocean
        public bool coast;  // touches ocean and land polygons
        public bool border;  // at the edge of the map
        public float elevation;  // 0.0-1.0
        public float moisture;  // 0.0-1.0

        [HideInInspector] public List<Center> touches;
        [HideInInspector] public List<Edge> protrudes;
        [HideInInspector] public List<Corner> adjacent;
        
    
        [HideInInspector] public int river;  // 0 if no river, or volume of water in river
        [HideInInspector] public Corner downslope;  // pointer to adjacent corner most downhill
        [HideInInspector] public Corner watershed;  // pointer to coastal corner, or null
        [HideInInspector] public int watershed_size;
        private Vector2f p0;


        public Corner(Vector2f p0, Site site)
        {
            this.p0 = p0;
            sites = new();
            
            sites.Add(site);
        }

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
    }

    public class Edge
    {
        public int index;
        public Center d0, d1;  // Delaunay edge
        public Corner v0, v1;  // Voronoi edge

        public List<Site> delaunaySites; // Any Delaunay triangle with this edge in its edges

        [HideInInspector] public Vector2f midpoint;  // halfway between v0,v1
        [HideInInspector] public int river;  // volume of water, or 0

        public Edge(Center d0, Center d1, Site site0, Site site1)
        {
            this.d0 = d0;
            this.d1 = d1;
            delaunaySites = new();

            delaunaySites.Add(site0);
            if(site0 != site1)
                delaunaySites.Add(site1);
        }

        public Edge(Corner v0, Corner v1)
        {
            this.v0 = v0;
            this.v1 = v1;
        }
    }

    public class Map
    {
        public List<Center> centers;
        public List<Corner> corners;
        public List<Edge> edges;

        public Map(List<Center> centers, List<Corner> corners, List<Edge> edges)
        {
            this.centers = centers;
            this.corners = corners;
            this.edges = edges;
        }
    }
}
