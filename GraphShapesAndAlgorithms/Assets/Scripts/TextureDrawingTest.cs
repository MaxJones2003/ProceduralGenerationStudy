using UnityEngine;
using Map;
using System.Collections.Generic;
public class TextureDrawingTest : MonoBehaviour
{
    public int mapWidth = 1000;
    public int mapHeight = 1000;

    public float minElevation = 0f;
    public float maxElevation = 1f;

    public void GenerateMap(List<Center> centers)
    {
        // Create a blank texture
        Texture2D texture = new Texture2D(mapWidth, mapHeight);
        GetComponent<Renderer>().sharedMaterial.mainTexture = texture;

        // Generate random sites with corners
        foreach (Center center in centers)
        {
            
            // Draw points on the texture
            foreach (Corner corner in center.corners)
            {
                int x = Mathf.RoundToInt(corner.point.x * mapWidth);
                int y = Mathf.RoundToInt(corner.point.y * mapHeight);
                texture.SetPixel(x, y, Color.white);
            }

            // Draw lines with gradient based on elevation
            foreach (Corner cornerA in center.corners)
            {
                foreach (Corner cornerB in cornerA.adjacent)
                {
                    DrawLine(texture, cornerA.point, cornerB.point, cornerA.elevation, cornerB.elevation);
                }
            }
        }

        // Apply changes to the texture
        texture.Apply();
    }

    void DrawLine(Texture2D texture, Vector2f pointA, Vector2f pointB, float elevationA, float elevationB)
    {
        Vector2 pA = (Vector2)pointA;
        Vector2 pB = (Vector2)pointB;

        int steps = Mathf.RoundToInt(Vector2.Distance(pA, pB) * mapWidth);

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector2 point = Vector2.Lerp(pA, pB, t);
            float elevation = Mathf.Lerp(elevationA, elevationB, t);

            int x = Mathf.RoundToInt(point.x * mapWidth);
            int y = Mathf.RoundToInt(point.y * mapHeight);

            // Use grayscale based on elevation
            texture.SetPixel(x, y, new Color(elevation, elevation, elevation));
        }
    }
}