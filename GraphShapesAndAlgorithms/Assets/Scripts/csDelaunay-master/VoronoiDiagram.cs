using UnityEngine;
using System.Collections.Generic;
 
using csDelaunay;
using System.Linq;
using Map;
using static IDWInterpolator;

public class VoronoiDiagram : MonoBehaviour {
    public Terrain terrain; // Drag and drop your Unity Terrain here in the Inspector.
    // The number of polygons/sites we want
    public IslandShapeEnum islandShape = IslandShapeEnum.Radial;
    public int polygonNumber = 200;
    public int iterations = 5;
 
    // This is where we will store the resulting data
    private Dictionary<Vector2f, Site> sites;
    //private Dictionary<Vector2f, Site> boundedSites;
    private List<csDelaunay.Edge> edges;
    public string seed;
    Rectf bounds;
    public Map.Map map;
    Mesh mesh;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
 

    public int stage;

    public List<Map.Center> centers;
    public List<Map.Corner> corners;
    public List<Map.Edge> mapEdges;
    /* private int terrainWidth = 100;
    private int terrainLength = 100;
    private float heightScale = 1000f; */


    Vector3[] meshVertices;
    int[] meshTriangles;
    public int SIZE = 1000;
    float[,] heights;

    Map.Grid grid;
    public bool generateRandom;
    public int step;
    public TextureDrawingTest textureDrawer;
    void Start()
    {
        GenerateVoronoi();
    }
    public void GenerateVoronoi()
    {
        string newSeed = generateRandom ? Seed.Instance.CreateRandomSeed(16) : seed;
        int seedInt = Seed.Instance.InitializeRandom(newSeed);
        bounds = new Rectf(0,0,SIZE,SIZE);
        map = new Map.Map(polygonNumber, SIZE, iterations, stage);
        map.NewIsland(islandShape, polygonNumber, seedInt);
        centers = map.centers;
        corners = map.corners;
        mapEdges = map.edges;
        grid = map.grid;
        Mesh mesh = GenerateMesh();
        GetComponent<MeshCollider>().sharedMesh = mesh;

        // any center that is ocean, set elevation to - 10 using lamda
        //centers.ForEach(c => c.elevation = c.ocean ? -10 : c.elevation);
        //corners.ForEach(c => c.elevation = c.ocean ? -10 : c.elevation);

        DebugReorderPoints();

        TerrainGenerator terrainGenerator = new TerrainGenerator(centers, corners, map.grid, terrain, SIZE, mesh);
        
        terrain.terrainData = terrainGenerator.GenerateTerrain();

        heights = new float[SIZE, SIZE];
        heights = terrain.terrainData.GetHeights(0, 0, SIZE, SIZE);

        textureDrawer.vD = this;
        textureDrawer.map = map;
        textureDrawer.GenerateMap(centers, corners, mapEdges, step, SIZE);
    }
    private float[,] heightMap;
    public List<Vector2f> CreateRandompoints() {
        // Use Vector2f, instead of Vector2
        // Vector2f is pretty much the same than Vector2, but like you could run Voronoi in another thread
        List<Vector2f> points = new List<Vector2f>();
        for (int i = 0; i < polygonNumber; i++) {
            points.Add(new Vector2f(Random.Range(0,512), Random.Range(0,512)));
        }
 
        return points;
    }
    Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>(); // List to store the color of each vertex

        foreach(Map.Center c in centers){
            if(c.ocean)
                continue;
            float zp = IDWInterpolator.InverseDistanceWeighting(c.point, c.corners, 2) * 50;
            vertices.Add(new Vector3(((Vector2)c.point).x,zp,((Vector2)c.point).y));
            int centerIndex=vertices.Count-1;
            var edges = c.borders;
            int lastIndex = 0;
            int firstIndex = 0;

            Color centerColor = GetBiomeColor(c); // Get the color for the center
            colors.Add(centerColor); // Add the color to the colors list

            for(int i =0;i<c.borders.Count;i++){
                if(edges[i].v0 == null && edges[i].v1 == null)
                    break;

                //get voronoi edge
                Corner corner0 = edges[i].v0;
                Corner corner1 = edges[i].v1;

                //get vertices height
                float z0 = corner0.elevation * 50;
                float z1 = corner1.elevation * 50;

                //creat voronoi edge points
                Vector3 v0 = new Vector3(((Vector2)corner0.point).x,z0,((Vector2)corner0.point).y);
                Vector3 v1 = new Vector3(((Vector2)corner1.point).x,z1,((Vector2)corner1.point).y);

                //add points to vertices
                vertices.Add(v0);
                var i2 = vertices.Count - 1;
                vertices.Add(v1);
                var i3 = vertices.Count - 1;

                //add colors for the new vertices
                colors.Add(GetBiomeColor(corner0));
                colors.Add(GetBiomeColor(corner1));

                //add triangles calculating surface normals so i can always add triangles clockwise correctly
                var surfaceNormal = Vector3.Cross (v0-(new Vector3(((Vector2)c.point).x,zp,((Vector2)c.point).y)), v1-(new Vector3(((Vector2)c.point).x,zp,((Vector2)c.point).y)));
                if(surfaceNormal.y>0)
                    AddTriangle(triangles, centerIndex, i2, i3);
                else
                    AddTriangle(triangles, centerIndex, i3, i2);
                    
                firstIndex = i2;
                lastIndex = i3;
            }
        }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            //mesh.SetColors(colors, 0, colors.Count); // Set the colors of the mesh

            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
            return mesh;
    }

    void DrawPolygons(List<Center> centers, int zScale){
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles=new List<int>();
        List<Color32> colors = new List<Color32>();

        foreach(Center c in centers){
            if(c.ocean)
                continue;
            float zp = zScale*c.elevation;
            vertices.Add(new Vector3(((Vector2)c.point).x,zp,((Vector2)c.point).y));
            Color c0 = GetBiomeColor(c);
            colors.Add(c0);
            int centerIndex=vertices.Count-1;
            var edges = c.borders;
            int lastIndex = 0;
            int firstIndex = 0;

            for(int i =0;i<c.borders.Count;i++){
                if(edges[i].v0 == null && edges[i].v1 == null)
                    break;

                //get voronoi edge
                Corner corner0 = edges[i].v0;
                Corner corner1 = edges[i].v1;

                //get vertices height
                float z0 = zScale*corner0.elevation * 20;
                float z1 = zScale*corner1.elevation * 20;

                //add color
                if(edges[i].river>0){
                    c0 = Color.cyan;
                }else{
                    c0 = GetBiomeColor(c);
                }
                colors.Add(c0);
                colors.Add(c0);

                //creat voronoi edge points
                Vector3 v0 = new Vector3(((Vector2)corner0.point).x,z0,((Vector2)corner0.point).y);
                Vector3 v1 = new Vector3(((Vector2)corner1.point).x,z1,((Vector2)corner1.point).y);

                //add points to vertices
                vertices.Add(v0);
                var i2 = vertices.Count - 1;
                vertices.Add(v1);
                var i3 = vertices.Count - 1;

                //add triangles calculating surface normals so i can always add triangles clockwise correctly
                var surfaceNormal = Vector3.Cross (v0-(new Vector3(((Vector2)c.point).x,zp,((Vector2)c.point).y)), v1-(new Vector3(((Vector2)c.point).x,zp,((Vector2)c.point).y)));
                if(surfaceNormal.y>0)
                    AddTriangle(triangles, centerIndex, i2, i3);
                else
                    AddTriangle(triangles, centerIndex, i3, i2);

                firstIndex = i2;
                lastIndex = i3;
            }
        }

        

        //calculating uv's
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / SIZE, vertices[i].z / SIZE);
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;
        mesh.colors32 = colors.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;

        
    }

    void AddTriangle(List<int> triangles, int index1, int index2, int index3)
    {
        triangles.Add(index1);
        triangles.Add(index2);
        triangles.Add(index3);
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
            if (p.elevation > 0.8) return Color.cyan; // Ice color
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
    public Color GetBiomeColor(EBiomeType p)
    {
        if (p == EBiomeType.OCEAN) return Color.blue;
        else if (p == EBiomeType.MARSH) return Color.green;
        else if (p == EBiomeType.SNOW) return Color.white;
        else if (p == EBiomeType.ICE) return Color.cyan;
        else if (p == EBiomeType.LAKE) return Color.blue;
        else if (p == EBiomeType.BEACH) return Color.yellow;
        else if (p == EBiomeType.SCORCHED) return Color.black;
        else if (p == EBiomeType.BARE) return new Color(0.5f, 0.5f, 0.5f);
        else if (p == EBiomeType.TUNDRA) return Color.gray;
        else if (p == EBiomeType.SHRUBLAND) return new Color(0.4f, 0.3f, 0.1f);
        else if (p == EBiomeType.TAIGA) return new Color(0.2f, 0.5f, 0.2f);
        else if (p == EBiomeType.GRASSLAND) return new Color(0.6f, 0.8f, 0.3f);
        else if (p == EBiomeType.TEMPERATE_DESERT) return Color.yellow;
        else if (p == EBiomeType.TEMPERATE_RAIN_FOREST) return new Color(0.1f, 0.4f, 0.1f);
        else if (p == EBiomeType.TEMPERATE_DECIDUOUS_FOREST) return new Color(0.2f, 0.6f, 0.2f);
        else if (p == EBiomeType.SUBTROPICAL_DESERT) return new Color(0.8f, 0.8f, 0.4f);
        else if (p == EBiomeType.TROPICAL_RAIN_FOREST) return new Color(0.0f, 0.2f, 0.0f);
        else if (p == EBiomeType.TROPICAL_SEASONAL_FOREST) return new Color(0.1f, 0.3f, 0.1f);
        else
        {
            Debug.LogError("No color found for biome type: " + p);
            return Color.magenta;
        }
    }

    public Color GetBiomeColor(Center p)
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
    //float scale = 10f;
    public Vector2Int drawGizmosCenter;
    public Vector2Int drawGizmosSize;
    public void DisplayHeightMapGizmos(Vector2Int center, Vector2Int range)
    {
        int startX = Mathf.Max(center.x - range.x / 2, 0);
        int endX = Mathf.Min(center.x + range.x / 2, heights.GetLength(0));

        int startY = Mathf.Max(center.y - range.y / 2, 0);
        int endY = Mathf.Min(center.y + range.y / 2, heights.GetLength(1));

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Gizmos.DrawSphere(new Vector3(x, heights[x, y] * 50, y), .1f);
            }
        }
    }

    private void DisplayCornerGizmos()
    {
        foreach (var corner in corners)
        {
            Gizmos.color = GetBiomeColor(corner);

            Gizmos.DrawSphere(new Vector3(corner.point.x, corner.elevation * 50, corner.point.y), 1f);
        }
    }

    private void DisplayCenterGizmos()
    {
        foreach (var center in centers)
        {
            Gizmos.color = GetBiomeColor(center);

            Gizmos.DrawSphere(new Vector3(center.point.x, center.elevation * 50, center.point.y), 1f);
        }
    }

    private void DisplayEdgeGizmos(bool cornerEdges, bool centerEdges, bool biomeColors)
    {
        foreach (var edge in mapEdges)
        {
            if (cornerEdges)
            {
                Gizmos.color = biomeColors ? BlendColors(GetBiomeColor(edge.d0), GetBiomeColor(edge.d1), 0.5f) : Color.white;

                Gizmos.DrawLine(new Vector3(edge.v0.point.x, edge.v0.elevation * 50, edge.v0.point.y), new Vector3(edge.v1.point.x, edge.v1.elevation * 50, edge.v1.point.y));
                Gizmos.color = biomeColors ? GetBiomeColor(edge.v0) : Color.blue;
                Gizmos.DrawSphere(new Vector3(edge.v0.point.x, edge.v0.elevation * 50, edge.v0.point.y), 1f);
                Gizmos.color = biomeColors ? GetBiomeColor(edge.v1) : Color.blue;
                Gizmos.DrawSphere(new Vector3(edge.v1.point.x, edge.v1.elevation * 50, edge.v1.point.y), 1f);
            }

            if (centerEdges)
            {
                if (edge.v0 != null && edge.v1 != null)
                {
                    Gizmos.color = biomeColors ? BlendColors(GetBiomeColor(edge.v0), GetBiomeColor(edge.v1), 0.5f) : Color.black;

                    Gizmos.DrawLine(new Vector3(edge.d0.point.x, edge.d0.elevation * 50, edge.d0.point.y), new Vector3(edge.d1.point.x, edge.d1.elevation * 50, edge.d1.point.y));

                    Gizmos.color = biomeColors ? GetBiomeColor(edge.d0) : Color.red;
                    Gizmos.DrawSphere(new Vector3(edge.d0.point.x, edge.d0.elevation * 50, edge.d0.point.y), 1f);
                    Gizmos.color = biomeColors ? GetBiomeColor(edge.d1) : Color.red;
                    Gizmos.DrawSphere(new Vector3(edge.d1.point.x, edge.d1.elevation * 50, edge.d1.point.y), 1f);
                }
            }
        }
    }
    public bool drawCorners, drawCenters, drawEdges, useBiomeColors;
    public int howManyCentersToDraw = 0;
    void OnDrawGizmos()
    {
        if(map == null) return;


        //DisplayHeightMapGizmos(drawGizmosCenter, drawGizmosSize);
        
        if(drawEdges)
        {
            DisplayEdgeGizmos(drawCorners, drawCenters, useBiomeColors);
        }
        else
        {
            if(drawCorners) DisplayCornerGizmos();
            if(drawCenters) DisplayCenterGizmos();
        }

        #region Debug Inside Check
        for (int i = 0; i < centers.Count; i++)
        {
            if(i > howManyCentersToDraw) break;

            Gizmos.color = centers[i].Inside(centers[i].point) ? Color.green : Color.red;
            Gizmos.DrawSphere(new Vector3(centers[i].point.x, 0, centers[i].point.y), 1f);

            Gizmos.color = Color.black;
            foreach(var corner in centers[i].corners)
            {

                Gizmos.DrawSphere(new Vector3(corner.point.x, 0, corner.point.y), 1f);
            }
        }
        #endregion

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(bounds.left, 0, bounds.top), new Vector3(bounds.right, 0, bounds.top));
        Gizmos.DrawLine(new Vector3(bounds.left, 0, bounds.bottom), new Vector3(bounds.right, 0, bounds.bottom));
        Gizmos.DrawLine(new Vector3(bounds.left, 0, bounds.top), new Vector3(bounds.left, 0, bounds.bottom));
        Gizmos.DrawLine(new Vector3(bounds.right, 0, bounds.top), new Vector3(bounds.right, 0, bounds.bottom));

    }

    public void DebugReorderPoints()
    {
        if(map == null) return;

        foreach(var center in centers)
        {
            center.OrderCornersClockwise();
        }
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

public class TerrainGenerator
{
    public TerrainGenerator(List<Center> centers, List<Corner> corners, Map.Grid grid, Terrain terrain, int SIZE, Mesh mesh)
    {
        this.centers = centers;
        this.corners = corners;
        this.grid = grid;
        this.terrain = terrain;
        terrainWidth = SIZE;
        terrainLength = SIZE;
        this.mesh = mesh;
    }
    int terrainWidth = 100;
    int terrainLength = 100;
    float heightScale = 50f;

    List<Center> centers;
    List<Corner> corners;
    Map.Grid grid;

    Mesh mesh;


    Terrain terrain;
    float[,] heights;
    public TerrainData GenerateTerrain()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain not assigned. Please assign a Unity Terrain in the Inspector.");
            return null;
        }

        return GenerateTerrainData3();
        
    }

    TerrainData GenerateTerrainData()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = terrainWidth + 1;
        terrainData.size = new Vector3(terrainWidth, heightScale, terrainLength);

        heights = new float[terrainWidth + 1, terrainLength + 1];

        foreach (var center in centers)
        {
            // if any corner is outstide the bounds, skip this center
            if (center.corners.Any(c => c.point.x < 0 || c.point.x > terrainWidth || c.point.y < 0 || c.point.y > terrainLength))
            {
                continue;
            }
            // Get the min and max x and y values of the corners of the center, rounded to int
            int xMin = Mathf.RoundToInt(center.corners.Min(c => c.point.x));
            int xMax = Mathf.RoundToInt(center.corners.Max(c => c.point.x));
            int yMin = Mathf.RoundToInt(center.corners.Min(c => c.point.y));
            int yMax = Mathf.RoundToInt(center.corners.Max(c => c.point.y));

            List<Vector2f> points = center.corners.Select(c => c.point).ToList();
            // iterate through the x and y values of the corners of the center
            IDWInterpolator iDWInterpolator = new IDWInterpolator(corners, 2, terrainWidth);
            for (int x = xMin; x <= xMax + 2; x++)
            {
                for (int y = yMin; y <= yMax + 2; y++)
                {
                    // Check if the point is inside the polygon
                    if (!PolygonChecker.IsPointInsidePolygon(new Vector2f(x, y), points) && (x >= 0 && x < terrainWidth && y >= 0 && y < terrainLength))
                    {
                        // Get the elevation of the point
                        //float elevation = ElevationCalculator.GetElevation(center, new Vector2f(x, y));
                        float elevation = iDWInterpolator.Interpolate(new Vector2f(x, y)); 
                        // Set the height of the point
                        heights[x, y] = elevation * 50;
                    }
                    
                }
            }
        }

        terrainData.SetHeights(0, 0, heights);

        return terrainData;
    }
    TerrainData GenerateTerrainData2()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = terrainWidth + 1;
        terrainData.size = new Vector3(terrainWidth, heightScale, terrainLength);

        heights = new float[terrainWidth + 1, terrainLength + 1];
        //int numOfNull = 0;
        // find the maximum height value of all corners
        float maxHeight = corners.Max(corner => corner.elevation);
        IDWInterpolator iDWInterpolator = new IDWInterpolator(corners, 2, terrainWidth);
        for(int x = 0; x < terrainWidth; x++)
        {
            for(int y = 0; y < terrainLength; y++)
            {
                /* List<Center> gridCenters = grid.FindCentersInGrid(x, y);
                if(gridCenters == null) continue;
                Center center = gridCenters.Find(c => IsPointInPolygon(c.corners, new Vector2f(x, y)));
                
                float elevation = center == null ? 0 : IDWInterpolator.InverseDistanceWeighting(new Vector2f(x, y), center.corners, 2); */
                float elevation = iDWInterpolator.InverseDistanceWeighting(new Vector2f(x, y));
                
                // normalize the elevation between 0 and 1 using the maxHeight
                elevation = Mathf.InverseLerp(0, maxHeight, elevation);
                //numOfNull += center == null ? 1 : 0;
                heights[x, y] = elevation;
            }
        }
        //Debug.Log(numOfNull);
        /* for(int x = 0; x < terrainWidth; x++)
        {
            for(int y = 0; y < terrainLength; y++)
            {
                if(heights[x, y] == 0)
                {
                    heights[x, y] = GetAverageElevation(x, y);
                }
            }
        } */
        /* int howmany = 0;
        foreach(var h in heights)  if(h == 0) howmany++;
        Debug.Log(howmany); */

        terrainData.SetHeights(0, 0, heights);

        return terrainData;
    }

    TerrainData GenerateTerrainData3()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = terrainWidth + 1;
        terrainData.size = new Vector3(terrainWidth, heightScale, terrainLength);

        //(float, EBiomeType)[,] map = HeightMapMaker.Go(25f, terrainLength + 1, terrainWidth + 1, corners, out float maxHeight, out float minHeight);
        heights = HeightMapMaker.GenerateHeightMapInterpolatedNormalizedNew(25f, terrainLength + 1, terrainWidth + 1, corners, out float maxHeight, out float minHeight);
        //heights = ConvertToFloatArray(map);
        //heights = HeightMapMaker.GenerateHeightMapInterpolatedNormalized(25f, terrainLength + 1, terrainWidth + 1, corners, out float maxHeight, out float minHeight);
        terrainData.size = new Vector3(terrainData.size.x, 50, terrainData.size.z);
        terrainData.SetHeights(0, 0, heights);
        //terrainData.SetAlphamaps(0, 01) changes textures???

        return terrainData;
    }
    float[,] ConvertToFloatArray((float, EBiomeType)[,] sourceArray)
    {
        int rows = sourceArray.GetLength(0);
        int cols = sourceArray.GetLength(1);

        float[,] resultArray = new float[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                resultArray[i, j] = sourceArray[i, j].Item1;
            }
        }

        return resultArray;
    }

    
    void GenerateTexture()
    {
        int textureWidth = terrainWidth;
        int textureHeight = terrainLength;
        Texture2D texture = new Texture2D(textureWidth, textureHeight);

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                // Convert the height map value to grayscale color
                float heightValue = heights[x, y];
                Color color = new Color(heightValue, heightValue, heightValue);

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply(); // Apply the changes to the texture

        // Assuming you have a reference to a material on a GameObject, you can assign the texture to it
        // make a new plane and apply the texture
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
        plane.GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
    }
    float GetAverageElevation(int x, int y)
    {
        float up, down, left, right, upLeft, upRight, downLeft, downRight;
        up = down = left = right = upLeft = upRight = downLeft = downRight = 0;

        up = (x >= 0 && x < terrainWidth && y + 1 >= 0 && y + 1 < terrainLength) ? heights[x, y + 1] : 0;
        down = (x >= 0 && x < terrainWidth && y - 1 >= 0 && y - 1 < terrainLength) ? heights[x, y - 1] : 0;
        left = (x - 1 >= 0 && x - 1 < terrainWidth && y >= 0 && y < terrainLength) ? heights[x - 1, y] : 0;
        right = (x + 1 >= 0 && x + 1 < terrainWidth && y >= 0 && y < terrainLength) ? heights[x + 1, y] : 0;
        upLeft = (x - 1 >= 0 && x - 1 < terrainWidth && y + 1 >= 0 && y + 1 < terrainLength) ? heights[x - 1, y + 1] : 0;
        upRight = (x + 1 >= 0 && x + 1 < terrainWidth && y + 1 >= 0 && y + 1 < terrainLength) ? heights[x + 1, y + 1] : 0;
        downLeft = (x - 1 >= 0 && x - 1 < terrainWidth && y - 1 >= 0 && y - 1 < terrainLength) ? heights[x - 1, y - 1] : 0;
        downRight = (x + 1 >= 0 && x + 1 < terrainWidth && y - 1 >= 0 && y - 1 < terrainLength) ? heights[x + 1, y - 1] : 0;

        return (up + down + left + right + upLeft + upRight + downLeft + downRight) / 8;
    }

    public static bool IsPointInPolygon(List<Corner> polygon, Vector2f testPoint)
    {
        List<Vector2f> points = new List<Vector2f>();
        polygon.ForEach(c => points.Add(c.point));
        return InsidePolygonHelper.checkInside(points, points.Count, testPoint) == 1;
    }
}

public static class ElevationCalculator
{
    public static float GetElevation(Center center, Vector2f point)
    {
        float elevation = BarycentricInterpolation(point, center.corners);

        return elevation;
    }

    private static float BarycentricInterpolation(Vector2f point, List<Corner> corners)
    {
        float sumWeights = 0;
        float sumElevationWeighted = 0;

        for (int i = 0; i < corners.Count; i++)
        {
            float weight = BarycentricWeight(point, corners, i);
            sumWeights += weight;
            sumElevationWeighted += weight * corners[i].elevation;
        }

        // Normalize the weights
        if (sumWeights != 0)
            return sumElevationWeighted / sumWeights;
        else
            return 0f; // Default value if all weights are zero
    }

    private static float BarycentricWeight(Vector2f point, List<Corner> corners, int index)
    {
        float product = 1;

        for (int i = 0; i < corners.Count; i++)
        {
            if (i != index)
            {
                float numerator = (point.x - corners[i].point.x) * (corners[index].point.y - corners[i].point.y) -
                                  (corners[index].point.x - corners[i].point.x) * (point.y - corners[i].point.y);

                float denominator = (corners[index].point.x - corners[i].point.x) * (corners[index].point.y - corners[i].point.y);

                if (denominator != 0)
                {
                    product *= numerator / denominator;
                }
            }
        }

        return product;
    }

    // Return True if the point is in the polygon.
    public static bool PointInPolygon(float X, float Y, List<Corner> corners)
    {
        // Get a list of Vector2f points from the corners list, use a lamda
        List<Vector2f> points = new List<Vector2f>();
        corners.ForEach(c => points.Add(c.point));

        // Get the angle between the point and the
        // first and last vertices.
        int max_point = points.Count - 1;
        float total_angle = GetAngle(
            points[max_point].x, points[max_point].y,
            X, Y,
            points[0].x, points[0].y);

        // Add the angles from the point
        // to each other pair of vertices.
        for (int i = 0; i < max_point; i++)
        {
            total_angle += GetAngle(
                points[i].x, points[i].y,
                X, Y,
                points[i + 1].x, points[i + 1].y);
        }

        // The total angle should be 2 * PI or -2 * PI if
        // the point is in the polygon and close to zero
        // if the point is outside the polygon.
        return Mathf.Abs(total_angle) > 0.000001;
    }

    // Return the angle ABC.
    // Return a value between PI and -PI.
    // Note that the value is the opposite of what you might
    // expect because Y coordinates increase downward.
    public static float GetAngle(float Ax, float Ay,
        float Bx, float By, float Cx, float Cy)
    {
        // Get the dot product.
        float dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);

        // Get the cross product length.
        float cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);

        // Calculate the angle.
        return (float)Mathf.Atan2(cross_product, dot_product);
    }

    // Calculate the cross product length.
    private static float CrossProductLength(float Ax, float Ay,
        float Bx, float By, float Cx, float Cy)
    {
        // Get the vectors' coordinates.
        float BAx = Ax - Bx;
        float BAy = Ay - By;
        float BCx = Cx - Bx;
        float BCy = Cy - By;

        // Calculate the cross product length.
        return (BAx * BCy - BAy * BCx);
    }

    // Return the dot product AB . BC.
    // Note that AB x BC = |AB| * |BC| * Cos(theta).
    private static float DotProduct(float Ax, float Ay,
        float Bx, float By, float Cx, float Cy)
    {
        // Get the vectors' coordinates.
        float BAx = Ax - Bx;
        float BAy = Ay - By;
        float BCx = Cx - Bx;
        float BCy = Cy - By;

        // Calculate the dot product.
        return (BAx * BCx + BAy * BCy);
    }
}