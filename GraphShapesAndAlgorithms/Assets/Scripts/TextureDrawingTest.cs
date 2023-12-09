using UnityEngine;
using Map;
using System.Collections.Generic;
using static UnityEngine.UI.GridLayoutGroup;

public class TextureDrawingTest : MonoBehaviour
{
    public VoronoiDiagram vD;
    public int mapSize;

    public float minElevation = 0f;
    public float maxElevation = 1f;
    Texture2D texture;
    public Map.Map map;
    public bool testRadial;
    public bool drawRivers;
    public bool drawBiomes;
    public bool drawMoisture;
    public Color baseHeightColor;

    public Color highMoistureColor;
    public Color lowMoistureColor;
    public void GenerateMap(List<Center> centers, List<Map.Corner> corners, List<Map.Edge> edges, int steps, int mapSize)
    {
        this.mapSize = mapSize;
        // Create a blank texture
        texture = new Texture2D(mapSize, mapSize);
        Material material = GetComponent<Renderer>().sharedMaterial;
        Color color = steps < 4 ? Color.grey : Color.blue;
        // Set the texture to blue
        if(!testRadial)
        {
            for(int i=0; i<texture.width; i++)
                for(int j=0; j<texture.height; j++)
                    texture.SetPixel(i, j, color);
                
            if(steps == 1)
            {
                DrawCenters(centers);
            }
            else if(steps == 2)
            {
                DrawCenters(centers);
                DrawTriangleLines(edges);
            }
            else if(steps == 3)
            {
                DrawCenters(centers);
                DrawTriangleLines(edges);
                DrawCorners(corners);
                DrawVoronoiLines(edges);
            }
            else if(steps == 4 && drawBiomes)
            {
                DrawVoronoiLines(edges);
                foreach(var c in centers)
                {
                    int x = Mathf.RoundToInt(c.point.x);
                    int y = Mathf.RoundToInt(c.point.y);
                    if (x < 0 || y < 0 || x > mapSize || y > mapSize) continue;
                    //texture.SetPixel(x, y, Color.white);
                    FloodFill(x, y, vD.GetBiomeColor(c.biomeEnum));
                }
            }
            else if(steps == 4 && !drawBiomes)
            {
                DrawVoronoiLines(edges);
                foreach(var c in centers)
                {
                    int x = Mathf.RoundToInt(c.point.x);
                    int y = Mathf.RoundToInt(c.point.y);
                    if (x < 0 || y < 0 || x > mapSize || y > mapSize) continue;
                    //texture.SetPixel(x, y, Color.white);
                    Color newColor = new Color();
                    newColor = !c.ocean ? (!drawMoisture ? Color.Lerp(baseHeightColor, Color.white, c.elevation) : Color.Lerp(lowMoistureColor, highMoistureColor, c.moisture)) : Color.blue;/* Color.green;
                    if(c.ocean) newColor = Color.blue; */
                    
                    FloodFill(x, y, newColor);
                }
            }
        }
        else
        {
            // This draws the noise map
            for(int i=0; i<texture.width; i++)
                for(int j=0; j<texture.height; j++)
                {
                    if(map.Inside(new Vector2f(i,j)))
                    {
                        texture.SetPixel(i, j, Color.black);
                    }
                }

        }



        // Apply changes to the texture
        texture.Apply();
        material.mainTexture = texture;
    }

    // FloodFill function, takes a position (within the texture size) and checks sorrounding pixels, if they are the same color, it changes them to the new color, 
    // it continues until it finds a different color, if the color is different, don't change it, it shouldn't stop until the entire area is filled.
    public void FloodFill(int x, int y, Color newColor)
    {
        Color oldColor = texture.GetPixel(x, y);
        if (oldColor == newColor) return;
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(x, y));
        while (q.Count > 0)
        {
            Vector2Int p = q.Dequeue();
            if (texture.GetPixel(p.x, p.y) != oldColor) continue;
            texture.SetPixel(p.x, p.y, newColor);
            q.Enqueue(new Vector2Int(p.x + 1, p.y));
            q.Enqueue(new Vector2Int(p.x - 1, p.y));
            q.Enqueue(new Vector2Int(p.x, p.y + 1));
            q.Enqueue(new Vector2Int(p.x, p.y - 1));
        }
        texture.Apply();
    }

    void DrawLine(Vector2f pointA, Vector2f pointB, float elevationA, float elevationB)
    {
        Vector2 pA = (Vector2)pointA;
        Vector2 pB = (Vector2)pointB;

        int steps = Mathf.RoundToInt(Vector2.Distance(pA, pB) * mapSize);

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 point = Vector2.Lerp(pA, pB, t);
            float elevation = Mathf.Lerp(elevationA, elevationB, t);

            int x = Mathf.RoundToInt(point.x * mapSize);
            int y = Mathf.RoundToInt(point.y * mapSize);

            // Use grayscale based on elevation
            texture.SetPixel(x, y, new Color(elevation, elevation, elevation));
        }
    }
    public void DrawCenters(List<Center> centers)
    {
        // Draw points on the texture
        foreach (var center in centers)
        {
            int x = Mathf.RoundToInt(center.point.x);
            int y = Mathf.RoundToInt(center.point.y);
            if (x < 0 || y < 0 || x > mapSize || y > mapSize) continue;
            //texture.SetPixel(x, y, Color.white);
            DrawLargePixel(x, y, Color.red, 2);
        }
    }
    public void DrawCorners(List<Map.Corner> corners)
    {
        // Draw points on the texture
        foreach (var corner in corners)
        {
            int x = Mathf.RoundToInt(corner.point.x);
            int y = Mathf.RoundToInt(corner.point.y);
            if (x < 0 || y < 0 || x > mapSize || y > mapSize) continue;
            //texture.SetPixel(x, y, Color.white);
            DrawLargePixel(x, y, Color.blue, 2);
        }
    }

    public void DrawTriangleLines(List<Edge> edges)
    {
        foreach (var edge in edges)
        {
            var d0 = edge.d0;
            var dP0 = d0.point;
            if (dP0.x < 0 || dP0.y < 0 || dP0.x > mapSize || dP0.y > mapSize) continue;
            var d1 = edge.d1;
            var dP1 = d1.point;
            if (dP1.x < 0 || dP1.y < 0 || dP1.x > mapSize || dP1.y > mapSize) continue;

            DrawLine(dP0, dP1,Color.black);
        }
    }

    public void DrawVoronoiLines(List<Edge> edges)
    {
        foreach (var edge in edges)
        {
            var v0 = edge.v0;
            var vP0 = v0.point;
            if (vP0.x < 0 || vP0.y < 0 || vP0.x > mapSize || vP0.y > mapSize) continue;
            var v1 = edge.v1;
            var vP1 = v1.point;
            if (vP1.x < 0 || vP1.y < 0 || vP1.x > mapSize || vP1.y > mapSize) continue;

            Color color = (edge.river > 0 && drawRivers) ? Color.cyan : Color.black;//new Color(0.01f, 0.01f, 0.01f);
            DrawLine(vP0, vP1, color);
        }
    }

    private void DrawLine(Vector2f p0, Vector2f p1, Color c, int offset = 0)
    {
        int x0 = (int)p0.x;
        int y0 = (int)p0.y;
        int x1 = (int)p1.x;
        int y1 = (int)p1.y;


        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            texture.SetPixel(x0 + offset, y0 + offset, c);

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
    private void DrawLargePixel(int x, int y, Color color, int size)
    {
        for (int i = -size; i <= size; i++)
        {
            for (int j = -size; j <= size; j++)
            {
                texture.SetPixel(x + i, y + j, color);
            }
        }
    }
}