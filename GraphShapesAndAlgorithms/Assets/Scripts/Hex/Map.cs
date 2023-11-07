using System;
using System.Collections.Generic;

/* public class Map
{
    public HashSet<Hex> hexes = new HashSet<Hex>();

    public void GenerateHexagonalGrid(int radius)
    {
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Math.Max(-radius, -q - radius);
            int r2 = Math.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                hexes.Add(new Hex(q, r, -q - r));
            }
        }
    }
    
    public void GenerateParallelogram(int q1, int q2, int r1, int r2)
    {
        for (int q = q1; q <= q2; q++)
        {
            for (int r = r1; r <= r2; r++)
            {
                hexes.Add(new Hex(q, r, -q - r));
            }
        }
    }

    public void GenerateTriangle1(int mapSize)
    {
        for (int q = 0; q <= mapSize; q++)
        {
            for (int r = 0; r <= mapSize - q; r++)
            {
                hexes.Add(new Hex(q, r, -q - r));
            }
        }
    }

    public void GenerateTriangle2(int mapSize)
    {
        for (int q = 0; q <= mapSize; q++)
        {
            for (int r = mapSize - q; r <= mapSize; r++)
            {
                hexes.Add(new Hex(q, r, -q - r));
            }
        }
    }

    public void GenerateHexagonalMap(int N)
    {
        for (int q = -N; q <= N; q++)
        {
            int r1 = Math.Max(-N, -q - N);
            int r2 = Math.Min(N, -q + N);
            for (int r = r1; r <= r2; r++)
            {
                hexes.Add(new Hex(q, r, -q - r));
            }
        }
    }
}
 */