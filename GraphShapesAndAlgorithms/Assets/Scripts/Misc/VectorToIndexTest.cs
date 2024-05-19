using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorToIndexTest : MonoBehaviour
{
    public int W = 16;
    public int L = 16;
    public int H = 16;
    void Start()
    {
        int i = 0;
        for(int x = 0; x < W; x++)
        {
            for(int z = 0; z < L; z++)
            {
                for(int y = 0; y < 16; y++)
                {
                    int index = IndexFromCoord3(new Vector3Int(x, y, z));
                    if(index == i) Debug.Log("Vertex: " + new Vector3Int(z, y, x) + ", Index: " + index + " is correct.");
                    if(index > 4351) Debug.LogError("Index too large at vertex: " + new Vector3Int(x, y, z));
                    i++;
                }
            }
        }
    }

    int IndexFromCoord3(Vector3Int coord)
    {
        return coord.x + W * (coord.y + L * coord.z);
    }

    
}
