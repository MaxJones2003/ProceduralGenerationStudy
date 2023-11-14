using System;
using UnityEngine;

public enum IslandShapeEnum
{
    Radial,
    Perlin,
    Square,
    Blob
}

public class IslandShape
{
    // This class has factory functions for generating islands of
    // different shapes. The factory returns a function that takes a
    // normalized point (x and y are -1 to +1) and returns true if the
    // point should be on the island, and false if it should be water
    // (lake or ocean).

    // The radial island radius is based on overlapping sine waves
    public static float ISLAND_FACTOR = 1.07f; // 1.0 means no small islands; 2.0 leads to a lot

    public static Func<Vector2f, bool> MakeRadial(int seed)
    {
        System.Random islandRandom = new System.Random(seed);
        int bumps = PRandom.NextIntRange(islandRandom, 1, 6);
        float startAngle = PRandom.NextDoubleRange(islandRandom, 0f, 2 * Mathf.PI);
        float dipAngle = PRandom.NextDoubleRange(islandRandom, 0f, 2 * Mathf.PI);
        float dipWidth = PRandom.NextDoubleRange(islandRandom, 0.2f, 0.7f);

        bool Inside(Vector2f q)
        {
            float angle = Mathf.Atan2(q.y, q.x);
            float length = 0.5f * (Mathf.Max(Mathf.Abs(q.x), Mathf.Abs(q.y)) + q.magnitude);

            float r1 = 0.5f + 0.40f * Mathf.Sin(startAngle + bumps * angle + Mathf.Cos((bumps + 3) * angle));
            float r2 = 0.7f - 0.20f * Mathf.Sin(startAngle + bumps * angle - Mathf.Sin((bumps + 2) * angle));
            if (Mathf.Abs(angle - dipAngle) < dipWidth
                || Mathf.Abs(angle - dipAngle + 2 * Mathf.PI) < dipWidth
                || Mathf.Abs(angle - dipAngle - 2 * Mathf.PI) < dipWidth)
            {
                r1 = r2 = 0.2f;
            }
            return length < r1 || (length > r1 * ISLAND_FACTOR && length < r2);
        }

        return Inside;
    }
    /* static public function makeRadial(seed:int):Function {
    var islandRandom:PM_PRNG = new PM_PRNG();
    islandRandom.seed = seed;
    var bumps:int = islandRandom.nextIntRange(1, 6);
    var startAngle:Number = islandRandom.nextDoubleRange(0, 2*Math.PI);
    var dipAngle:Number = islandRandom.nextDoubleRange(0, 2*Math.PI);
    var dipWidth:Number = islandRandom.nextDoubleRange(0.2, 0.7);
    
    function inside(q:Point):Boolean {
      var angle:Number = Math.atan2(q.y, q.x);
      var length:Number = 0.5 * (Math.max(Math.abs(q.x), Math.abs(q.y)) + q.length);

      var r1:Number = 0.5 + 0.40*Math.sin(startAngle + bumps*angle + Math.cos((bumps+3)*angle));
      var r2:Number = 0.7 - 0.20*Math.sin(startAngle + bumps*angle - Math.sin((bumps+2)*angle));
      if (Math.abs(angle - dipAngle) < dipWidth
          || Math.abs(angle - dipAngle + 2*Math.PI) < dipWidth
          || Math.abs(angle - dipAngle - 2*Math.PI) < dipWidth) {
        r1 = r2 = 0.2;
      }
      return  (length < r1 || (length > r1*ISLAND_FACTOR && length < r2));
    }

    return inside;
  } */
    

    // The Perlin-based island combines perlin noise with the radius
    public static Func<Vector2f, bool> MakePerlin(int seed)
    {
        Texture2D perlinTexture = new Texture2D(512, 512);
        perlinTexture.SetPixels(GeneratePerlinNoise(seed));
        perlinTexture.Apply();

        bool Inside(Vector2f q)
        {
            float c = perlinTexture.GetPixel((int)((q.x + 1) * 128), (int)((q.y + 1) * 128)).r;
            return c > (0.3f + 0.3f * q.magnitude * q.magnitude);
        }

        return Inside;
    }

    private static Color[] GeneratePerlinNoise(int seed)
    {
        UnityEngine.Random.InitState(seed);
        Color[] pixels = new Color[256 * 256];

        for (int y = 0; y < 256; y++)
        {
            for (int x = 0; x < 256; x++)
            {
                float c = UnityEngine.Random.Range(0f, 1f);
                pixels[y * 256 + x] = new Color(c, c, c);
            }
        }

        return pixels;
    }

    // The square shape fills the entire space with land
    public static Func<Vector2f, bool> MakeSquare(int seed)
    {
        return q => true;
    }

    // The blob island is shaped like Amit's blob logo
    public static Func<Vector2f, bool> MakeBlob(int seed)
    {
        return q =>
        {
            bool eye1 = new Vector2(q.x - 0.2f, q.y / 2 + 0.2f).magnitude < 0.05f;
            bool eye2 = new Vector2(q.x + 0.2f, q.y / 2 + 0.2f).magnitude < 0.05f;
            bool body = q.magnitude < 0.8f - 0.18f * Mathf.Sin(5 * Mathf.Atan2(q.y, q.x));
            return body && !eye1 && !eye2;
        };
    }
}
