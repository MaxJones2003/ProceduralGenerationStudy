using UnityEngine;

public class HeatmapGenerator : MonoBehaviour
{
    public int width = 256;
    public int height = 256;
    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;

    public Color hotColor = Color.red;
    public Color coldColor = Color.blue;

    void Start()
    {
        offsetX = Random.Range(0f, 99999f);
        offsetY = Random.Range(0f, 99999f);
    }

    void Update()
    {
        
    }

    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = CalculateColor(x, y);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    Color CalculateColor(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        return Color.Lerp(coldColor, hotColor, sample);
    }

    private void OnValidate() {
        
        Renderer renderer = GetComponent<Renderer>();
        renderer.sharedMaterial.mainTexture = GenerateTexture();
    }
}