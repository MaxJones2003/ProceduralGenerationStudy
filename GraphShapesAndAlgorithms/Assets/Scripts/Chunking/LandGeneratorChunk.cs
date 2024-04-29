using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandGeneratorChunk : AbstractChunk
{
    private string seed;
    private EIslandType islandType;
    private EIslandInfluences islandInfluence;
    public override void GenerateMesh()
    {
        
    }

    public override float[,] GetHeightMapData()
    {
        return null;
    }
}


public enum EIslandType
{
    Uninhabbited,
    Inhabited,
    Barren
}

public enum EIslandInfluences
{
    Tropical,
    Hot,
    Cold,
    Frozen,
    Desert,
    Forrest
}