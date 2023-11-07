using UnityEngine;
using System.Collections.Generic;
 
using csDelaunay;
using System.Linq;
using Map;

public class VoronoiDiagram : MonoBehaviour {
 
    // The number of polygons/sites we want
    public int polygonNumber = 200;
    public int iterations = 5;
 
    // This is where we will store the resulting data
    private Dictionary<Vector2f, Site> sites;
    private Dictionary<Vector2f, Site> boundedSites;
    private List<csDelaunay.Edge> edges;
    Voronoi voronoi;
    Rectf bounds;
 
    public void GenerateVoronoi() {
        // Create your sites (lets call that the center of your polygons)
        List<Vector2f> points = CreateRandomPoint();
       
        // Create the bounds of the voronoi diagram
        // Use Rectf instead of Rect; it's a struct just like Rect and does pretty much the same,
        // but like that it allows you to run the delaunay library outside of unity (which mean also in another tread)
        bounds = new Rectf(0,0,512,512);
       
        // There is a two ways you can create the voronoi diagram: with or without the lloyd relaxation
        // Here I used it with 2 iterations of the lloyd relaxation
        voronoi = new Voronoi(points,bounds,iterations);

 
        // But you could also create it without lloyd relaxtion and call that function later if you want
        //Voronoi voronoi = new Voronoi(points,bounds);
        //voronoi.LloydRelaxation(5);
 
        // Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
        sites = voronoi.SitesIndexedByLocation;
        edges = voronoi.Edges;
        FindBoundSites();
        CreateMap();
        DisplayVoronoiDiagram();
    }
    private List<Vector2f> CreateRandomPoint() {
        // Use Vector2f, instead of Vector2
        // Vector2f is pretty much the same than Vector2, but like you could run Voronoi in another thread
        List<Vector2f> points = new List<Vector2f>();
        for (int i = 0; i < polygonNumber; i++) {
            points.Add(new Vector2f(Random.Range(0,512), Random.Range(0,512)));
        }
 
        return points;
    }
 
    // Here is a very simple way to display the result using a simple bresenham line algorithm
    // Just attach this script to a quad
    private void DisplayVoronoiDiagram() {
        Texture2D tx = new Texture2D(512,512);
        foreach (KeyValuePair<Vector2f,Site> kv in sites) {
            DrawLargePixel(tx, (int)kv.Key.x, (int)kv.Key.y, Color.red, 1);
        }
        foreach (csDelaunay.Edge edge in edges) {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;
 
            DrawLine(edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT], tx, Color.black);
        }
        tx.Apply();
 
        GetComponent<Renderer>().sharedMaterial.mainTexture = tx;
    }
 
   
    // Bresenham line algorithm
    private void DrawLine(Vector2f p0, Vector2f p1, Texture2D tx, Color c, int offset = 0) {
        int x0 = (int)p0.x;
        int y0 = (int)p0.y;
        int x1 = (int)p1.x;
        int y1 = (int)p1.y;
        
       
        int dx = Mathf.Abs(x1-x0);
        int dy = Mathf.Abs(y1-y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx-dy;
       
        while (true) {
            tx.SetPixel(x0+offset,y0+offset,c);
           
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2*err;
            if (e2 > -dy) {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx) {
                err += dx;
                y0 += sy;
            }
        }
        DrawLargePixel(tx, (int)p0.x, (int)p0.y, Color.blue, 1);
        DrawLargePixel(tx, (int)p1.x, (int)p1.y, Color.blue, 1);
    }
    private void DrawLargePixel(Texture2D tx, int x, int y, Color color, int size)
    {
        for (int i = -size; i <= size; i++)
        {
            for (int j = -size; j <= size; j++)
            {
                tx.SetPixel(x + i, y + j, color);
            }
        }
    }

    public void FindBoundSites()
    {
        
        boundedSites = new();

        foreach(var siteDic in sites)
        {
            Site site = siteDic.Value;
            foreach(csDelaunay.Edge edge in site.Edges)
            {
                if(edge.ClippedEnds == null) continue;

                if(BoundsCheck.Check(edge.ClippedEnds[LR.LEFT], bounds) != 0)
                {
                    boundedSites.Add(siteDic.Key, site);
                    break;
                }
                else if(BoundsCheck.Check(edge.ClippedEnds[LR.RIGHT], bounds) != 0)
                {
                    boundedSites.Add(siteDic.Key, site);
                    break;
                }
            }
        }
    }

    public Map.Map map;
    public void CreateMap()
    {
        List<Center> centers = new();
        List<Corner> corners = new();
        List<Map.Edge> edges = new();
        foreach(Triangle tri in voronoi.Triangles)
        {
            // draw a line through each point
            Vector2f p0 = tri.Sites[0].Coord;
            Vector2f p1 = tri.Sites[1].Coord;
            Vector2f p2 = tri.Sites[2].Coord;

            // Add the Delaunay Centers
            Center c0 = new Center(p0, tri.Sites[0]);
            Center c1 = new Center(p1, tri.Sites[1]);
            Center c2 = new Center(p2, tri.Sites[2]);

            if(!centers.Any(c => c == c0)) centers.Add(c0);
            else centers.Find(c => c == c0).sites.Add(tri.Sites[0]);

            if(!centers.Any(c => c == c1)) centers.Add(c1);
            else centers.Find(c => c == c1).sites.Add(tri.Sites[1]);

            if(!centers.Any(c => c == c2)) centers.Add(c2);
            else centers.Find(c => c == c2).sites.Add(tri.Sites[2]);

            // Add the Delaunay Edges The Voronoi edges will be added later.
            if(!edges.Any(e => (e.d0 == p0 && e.d1 == p1) || (e.d0 == p1 && e.d1 == p0)))
            {
                edges.Add(new Map.Edge(c0, c1, tri.Sites[0], tri.Sites[1]));
            }
            else if(edges.Any(e => e.d0 == p0 || e.d1 == p0))
            {
                List<Map.Edge> foundEdges = edges.FindAll(e => e.d0 == p0 || e.d1 == p0);
                foreach(var edge in foundEdges) edge.delaunaySites.Add(tri.Sites[0]);
            }
            else if(edges.Any(e => e.d1 == p1 || e.d0 == p1))
            {
                List<Map.Edge> foundEdges = edges.FindAll(e => e.d1 == p1 || e.d0 == p1);
                foreach(var edge in foundEdges) edge.delaunaySites.Add(tri.Sites[1]);
            }
            if(!edges.Any(e => (e.d0 == p1 && e.d1 == p2) || (e.d0 == p2 && e.d1 == p1)))
            {
                edges.Add(new Map.Edge(c1, c2, tri.Sites[1], tri.Sites[2]));
            }
            else if(edges.Any(e => e.d0 == p1 || e.d1 == p1))
            {
                List<Map.Edge> foundEdges = edges.FindAll(e => e.d0 == p1 || e.d1 == p1);
                foreach(var edge in foundEdges) edge.delaunaySites.Add(tri.Sites[1]);
            }
            else if(edges.Any(e => e.d1 == p2 || e.d0 == p2))
            {
                List<Map.Edge> foundEdges = edges.FindAll(e => e.d1 == p2 || e.d0 == p2);
                foreach(var edge in foundEdges) edge.delaunaySites.Add(tri.Sites[2]);
            }
            if(!edges.Any(e => (e.d0 == p2 && e.d1 == p0) || (e.d0 == p0 && e.d1 == p2)))
            {
                edges.Add(new Map.Edge(c2, c0, tri.Sites[2], tri.Sites[0]));
            }
            else if (edges.Any(e => e.d0 == p2 || e.d1 == p2))
            {
                List<Map.Edge> foundEdges = edges.FindAll(e => e.d0 == p2 || e.d1 == p2);
                foreach(var edge in foundEdges) edge.delaunaySites.Add(tri.Sites[2]);
            }
            else if(edges.Any(e => e.d1 == p0 || e.d0 == p0))
            {
                List<Map.Edge> foundEdges = edges.FindAll(e => e.d1 == p0 || e.d0 == p0);
                foreach(var edge in foundEdges) edge.delaunaySites.Add(tri.Sites[0]);
            }
        }
        foreach(var site in sites.Values)
        {
            foreach(csDelaunay.Edge edge in site.Edges)
            {
                if(edge.ClippedEnds == null) continue;
                Vector2f p0 = edge.ClippedEnds[LR.LEFT];
                Vector2f p1 = edge.ClippedEnds[LR.RIGHT];
                Corner c0 = new Corner(p0, site);
                Corner c1 = new Corner(p1, site);

                if(!corners.Any(c => c == c0)) corners.Add(c0);
                else corners.Find(c => c == c0).sites.Add(site);
                
                if(!corners.Any(c => c == c1)) corners.Add(c1);
                else corners.Find(c => c == c1).sites.Add(site);

                 
            }
        }

        map = new Map.Map(centers, corners, edges);
    }

    public int polygonsToDisplay = 1;
    void OnDrawGizmos()
    {
        if(sites == null) return;
        foreach (var site in sites.Values)
        {
            Vector3 pos = new Vector3(site.x, 0, site.y)/50f - new Vector3(5f, 0, 5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(pos, 0.05f);
            foreach(csDelaunay.Edge edge in site.Edges)
            {
                if(edge.ClippedEnds == null) continue;
                Vector3 p0 = new Vector3(edge.ClippedEnds[LR.LEFT].x, 0, edge.ClippedEnds[LR.LEFT].y)/50f - new Vector3(5f, 0, 5f);
                Vector3 p1 = new Vector3(edge.ClippedEnds[LR.RIGHT].x, 0, edge.ClippedEnds[LR.RIGHT].y)/50f - new Vector3(5f, 0, 5f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(p0, 0.05f);
                Gizmos.DrawSphere(p1, 0.05f);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(p0, p1);
            }
        }
        foreach(Triangle tri in voronoi.Triangles)
        {
            // draw a line through each point
            Vector3 p0 = new Vector3(tri.Sites[0].x, 0, tri.Sites[0].y)/50f - new Vector3(5f, 0, 5f);
            Vector3 p1 = new Vector3(tri.Sites[1].x, 0, tri.Sites[1].y)/50f - new Vector3(5f, 0, 5f);
            Vector3 p2 = new Vector3(tri.Sites[2].x, 0, tri.Sites[2].y)/50f - new Vector3(5f, 0, 5f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(p0, 0.05f);
            Gizmos.DrawSphere(p1, 0.05f);
            Gizmos.DrawSphere(p2, 0.05f);

            Gizmos.color = Color.black;
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p0);
        }

        
    }
    /* void OnValidate()
    {
        if(polygonsToDisplay < 1) polygonsToDisplay = 1;
        if(polygonsToDisplay > sites.Count) polygonsToDisplay = sites.Count;
    } */
}      