using UnityEngine;
using System.Collections.Generic;
 
using csDelaunay;
using System.Linq;
using Map;

public class VoronoiDiagram : MonoBehaviour {
 
    // The number of polygons/sites we want
    public IslandShapeEnum islandShape = IslandShapeEnum.Radial;
    public int polygonNumber = 200;
    public int iterations = 5;
 
    // This is where we will store the resulting data
    private Dictionary<Vector2f, Site> sites;
    private Dictionary<Vector2f, Site> boundedSites;
    private List<csDelaunay.Edge> edges;
    public string seed;
    Rectf bounds;
    Map.Map map;
 

    public int stage;

    public List<Map.Center> mapCenters;
    public List<Map.Corner> mapCorners;
    public List<Map.Edge> mapEdges;


    Vector3[] meshVertices;
    int[] meshTriangles;

    public void GenerateVoronoi()
    {
        int seedInt = Seed.Instance.InitializeRandom(seed);
        bounds = new Rectf(0,0,1000,1000);
        map = new Map.Map(polygonNumber, 1000, iterations, stage);
        map.NewIsland(islandShape, polygonNumber, seedInt);
        mapCenters = map.centers;
        mapCorners = map.corners;
        mapEdges = map.edges;

        GenerateMesh(map.corners);
    }
    public List<Vector2f> CreateRandomPoints() {
        // Use Vector2f, instead of Vector2
        // Vector2f is pretty much the same than Vector2, but like you could run Voronoi in another thread
        List<Vector2f> points = new List<Vector2f>();
        for (int i = 0; i < polygonNumber; i++) {
            points.Add(new Vector2f(Random.Range(0,512), Random.Range(0,512)));
        }
 
        return points;
    }

    public void GenerateMesh(List<Corner> corners)
    {
        // Create a new mesh
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Vertices array to hold the corner positions
        Vector3[] vertices = new Vector3[corners.Count];

        // Assign positions and adjust height based on elevation
        for (int i = 0; i < corners.Count; i++)
        {
            vertices[i] = new Vector3(corners[i].point.x, corners[i].elevation, corners[i].point.y);
        }

        // Triangles array to define the mesh topology
        int[] triangles = new int[(corners.Count - 2) * 3];

        // Triangulate the corners
        for (int i = 0, j = 0; i < triangles.Length; i += 3, j++)
        {
            triangles[i] = 0;
            triangles[i + 1] = j + 1;
            triangles[i + 2] = j + 2;
        }

        // Assign the vertices and triangles to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Recalculate normals for lighting
        mesh.RecalculateNormals();
    }
 
    // Here is a very simple way to display the result using a simple bresenham line algorithm
    // Just attach this script to a quad
    private void DisplayVoronoiDiagram() {
        Texture2D tx = new Texture2D(1000,1000);
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


    public Color GetBiomeColor(Corner p)
    {
        if (p.ocean)
        {
            return Color.blue; // You can replace this with your desired ocean color
        }
        else if (p.water)
        {
            if (p.elevation < 0.1) return Color.green; // Marsh color
            if (p.elevation > 0.8) return Color.white; // Ice color
            return Color.blue; // Lake color
        }
        else if (p.coast)
        {
            return Color.yellow; // Beach color
        }
        else if (p.elevation > 0.8)
        {
            if (p.moisture > 0.50) return Color.white; // Snow color
            else if (p.moisture > 0.33) return Color.gray; // Tundra color
            else if (p.moisture > 0.16) return new Color(0.5f, 0.5f, 0.5f); // Bare color
            else return Color.black; // Scorched color
        }
        else if (p.elevation > 0.6)
        {
            if (p.moisture > 0.66) return new Color(0.2f, 0.5f, 0.2f); // Taiga color
            else if (p.moisture > 0.33) return new Color(0.4f, 0.3f, 0.1f); // Shrubland color
            else return Color.yellow; // Temperate Desert color
        }
        else if (p.elevation > 0.3)
        {
            if (p.moisture > 0.83) return new Color(0.1f, 0.4f, 0.1f); // Temperate Rain Forest color
            else if (p.moisture > 0.50) return new Color(0.2f, 0.6f, 0.2f); // Temperate Deciduous Forest color
            else if (p.moisture > 0.16) return new Color(0.6f, 0.8f, 0.3f); // Grassland color
            else return Color.yellow; // Temperate Desert color
        }
        else
        {
            if (p.moisture > 0.66) return new Color(0.0f, 0.2f, 0.0f); // Tropical Rain Forest color
            else if (p.moisture > 0.33) return new Color(0.1f, 0.3f, 0.1f); // Tropical Seasonal Forest color
            else if (p.moisture > 0.16) return new Color(0.6f, 0.8f, 0.3f); // Grassland color
            else return new Color(0.8f, 0.8f, 0.4f); // Subtropical Desert color
        }
    }

    public Color BlendColors(Color color1, Color color2, float blendFactor)
    {
        blendFactor = Mathf.Clamp01(blendFactor);

        float r = Mathf.Lerp(color1.r, color2.r, blendFactor);
        float g = Mathf.Lerp(color1.g, color2.g, blendFactor);
        float b = Mathf.Lerp(color1.b, color2.b, blendFactor);
        float a = Mathf.Lerp(color1.a, color2.a, blendFactor);

        return new Color(r, g, b, a);
    }


    #region Gizmos
    void OnDrawGizmos()
    {
        if(map == null) return;


        
        
        foreach(var corner in map.corners)
        {
            Gizmos.color = GetBiomeColor(corner);

            Gizmos.DrawSphere(new Vector3(corner.point.x/100, corner.elevation, corner.point.y/100), 0.05f);
        } 

        // only draw the edges that are within the border
        
        foreach(Map.Edge edge in map.edges)
        {
            /* // draw a red sphere at the d0 and d1 points
            Gizmos.color = Color.red;

            Gizmos.DrawSphere(new Vector3(edge.d0.point.x/100, edge.d0.elevation, edge.d0.point.y/100), 0.01f);

            Gizmos.DrawSphere(new Vector3(edge.d1.point.x/100, edge.d1.elevation, edge.d1.point.y/100), 0.01f); 
            // draw a black line between d0 and d1
            Gizmos.color = Color.black;
            Gizmos.DrawLine(new Vector3(edge.d0.point.x/100, edge.d0.elevation, edge.d0.point.y/100), new Vector3(edge.d1.point.x/100, edge.d1.elevation, edge.d1.point.y/100)); */

            // draw a blue sphere at the v0 and v1 points
            // draw a white line between v0 and v1
            if(edge.v0 != null && edge.v1 != null)
            {
                if(edge.river > 0) Gizmos.color = Color.blue;
                else Gizmos.color = BlendColors(GetBiomeColor(edge.v0), GetBiomeColor(edge.v1), 0.5f);
                Gizmos.DrawLine(new Vector3(edge.v0.point.x/100, edge.v0.elevation, edge.v0.point.y/100), new Vector3(edge.v1.point.x/100, edge.v1.elevation, edge.v1.point.y/100));
            }
        }

        // Draw a boundry with the Rectf bounds it has a float for the top bottom left and right

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(bounds.left/100, 0, bounds.top/100), new Vector3(bounds.right/100, 0, bounds.top/100));
        Gizmos.DrawLine(new Vector3(bounds.left/100, 0, bounds.bottom/100), new Vector3(bounds.right/100, 0, bounds.bottom/100));
        Gizmos.DrawLine(new Vector3(bounds.left/100, 0, bounds.top/100), new Vector3(bounds.left/100, 0, bounds.bottom/100));
        Gizmos.DrawLine(new Vector3(bounds.right/100, 0, bounds.top/100), new Vector3(bounds.right/100, 0, bounds.bottom/100));
    }
    public int displayPointAtIndex = 0;
    private Corner currentCorner;
    private List<Corner> displayCorners = new List<Corner>();
    private Center currentCenter;
    private List<Center> displayCenters = new List<Center>();
    private List<Map.Edge> displayCenterEdges = new List<Map.Edge>();
    private List<Map.Edge> displayEdges = new List<Map.Edge>();
    /* void OnValidate()
    {
        if(displayPointAtIndex < 0) displayPointAtIndex = 0;
        if(displayPointAtIndex >= mapCorners.Count) displayPointAtIndex = mapCorners.Count - 1;

        currentCorner = mapCorners[displayPointAtIndex];
        displayCorners = currentCorner.adjacent;
        displayEdges = currentCorner.protrudes;

        currentCenter = mapCenters[displayPointAtIndex];
        displayCenters = currentCenter.neighbors;
        displayCenterEdges = currentCenter.borders;
    } */
    #endregion
}      