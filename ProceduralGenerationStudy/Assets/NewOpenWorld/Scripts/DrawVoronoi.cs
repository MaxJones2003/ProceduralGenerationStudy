using UnityEngine;
using Habrador_Computational_Geometry;
public static class DrawVoronoi
{
    // Define the static method to draw Voronoi edges on a Texture2D
    public static void DrawEdges(Texture2D texture, VoronoiEdge2[] voronoiEdges, int textureWidth, int textureHeight, MyVector2 minVector2, MyVector2 maxVector2)
    {
        Color[] pixels = new Color[textureWidth * textureHeight];

        // Calculate scaling factors for x and y coordinates
        float scaleX = textureWidth / (maxVector2.x - minVector2.x);
        float scaleY = textureHeight / (maxVector2.y - minVector2.y);

        // Initialize all pixels to white
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        foreach (VoronoiEdge2 line in voronoiEdges)
        {
            MyVector2 start = line.p1;
            MyVector2 end = line.p2;

            // Scale the coordinates to fit within the texture
            int startX = Mathf.RoundToInt((start.x - minVector2.x) * scaleX);
            int startY = Mathf.RoundToInt((start.y - minVector2.y) * scaleY);
            int endX = Mathf.RoundToInt((end.x - minVector2.x) * scaleX);
            int endY = Mathf.RoundToInt((end.y - minVector2.y) * scaleY);

            DrawLine(ref pixels, textureWidth, textureHeight, startX, startY, endX, endY, Color.black);
        }

        // Apply the changes to the texture
        texture.SetPixels(pixels);
        texture.Apply();
    }   

    public static void DrawLine(ref Color[] pixels, int textureWidth, int textureHeight, int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            int index = y0 * textureWidth + x0;

            // Check if the coordinates are within the texture bounds
            if (x0 >= 0 && x0 < textureWidth && y0 >= 0 && y0 < textureHeight)
            {
                pixels[index] = color;
            }

            if (x0 == x1 && y0 == y1)
            {
                break;
            }

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

}
