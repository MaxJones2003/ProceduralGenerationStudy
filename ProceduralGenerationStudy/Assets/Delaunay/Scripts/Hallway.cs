using UnityEngine;
using System.Collections.Generic;

public class Hallway
{
    public Vector2Int From;
    public Vector2Int To;

    // If any hallways have identical from Vector2Int's, merge them, then use these variables to alter how the pathfinding connection works
    public bool MultiConnectionHall = false;
    public List<Vector2Int> MultiConnectionPointList;
    // The pathfinding algorithm should connect the from and to points, then branch off from the newly created hall to any extra locations

    // Created with a pathfinder
    public List<Vector2Int> HallwayGridPositions; 

    public void Merge(Hallway hallwayToMergeWith)
    {
        MultiConnectionHall = true;

        MultiConnectionPointList = new List<Vector2Int>();
        MultiConnectionPointList.Add(From);
        MultiConnectionPointList.Add(To);
        MultiConnectionPointList.Add(hallwayToMergeWith.To);
    }
}
