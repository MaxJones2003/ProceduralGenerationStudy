using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public abstract class AbstractChunk : MonoBehaviour
{
    public ESubChunkType ChunkType;
    public float[,] HeightMap;

    public abstract void GenerateMesh();
    public abstract float[,] GetHeightMapData();
}


public enum EChunkType
{
    Water,
    Land,
    LandGenerator
}

public enum ESubChunkType
{
    //Land
    BasicIsland,
    //Water
    BasicWater
}
