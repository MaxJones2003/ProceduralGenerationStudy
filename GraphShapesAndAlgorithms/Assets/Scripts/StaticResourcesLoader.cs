using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticResourcesLoader
{
    public static Material material;
    public static Material Material
    {
        get
        {
            if(material == null)
            {
                material = Resources.Load<Material>("Materials/Material");
            }
            return material;
        }
    }
    private static ComputeShader heightMapCompute;
    public static ComputeShader HeightMapCompute
    {
        get
        {
            if(heightMapCompute == null)
            {
                heightMapCompute = Resources.Load<ComputeShader>("ComputeShaders/HeightMapCompute");
            }
            return heightMapCompute;
        }
    }
    private static ComputeShader marchingCompute;
    public static ComputeShader MarchingCompute
    {
        get
        {
            if(heightMapCompute == null)
            {
                marchingCompute = Resources.Load<ComputeShader>("ComputeShaders/MarchingCompute");
            }
            return marchingCompute;
        }
    }
    private static ComputeShader gaussianBlurCompute;
    public static ComputeShader GaussianBlurCompute
    {
        get
        {
            if(gaussianBlurCompute == null)
            {
                gaussianBlurCompute = Resources.Load<ComputeShader>("ComputeShaders/GaussianBlurCompute");
            }
            return gaussianBlurCompute;
        }
    }
    private static ComputeShader heightMapToCubesCompute;
    public static ComputeShader HeightMapToCubesCompute
    {
        get
        {
            if(heightMapToCubesCompute == null)
            {
                heightMapToCubesCompute = Resources.Load<ComputeShader>("ComputeShaders/HeightMapToCubesCompute");
            }
            return heightMapToCubesCompute;
        }
    }



    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void LoadStaticAssets()
    {
        heightMapCompute = Resources.Load<ComputeShader>("ComputeShaders/HeightMapCompute");
        marchingCompute = Resources.Load<ComputeShader>("ComputeShaders/MarchingCompute");
        gaussianBlurCompute = Resources.Load<ComputeShader>("ComputeShaders/GaussianBlurCompute");
        heightMapToCubesCompute = Resources.Load<ComputeShader>("ComputeShaders/HeightMapToCubesCompute");
    }
}
