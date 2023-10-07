using UnityEngine;
using System.Collections.Generic;

public class Hallway
{
    public int fromRoomNumber;
    public Vector2Int From;
    public int toRoomNumber;
    public Vector2Int To;

    // If any hallways have identical from Vector2Int's, merge them, then use these variables to alter how the pathfinding connection works
    public bool MultiConnectionHall = false;
    public Dictionary<int, Vector2Int> MultiConnectionPointList;
    // The pathfinding algorithm should connect the from and to points, then branch off from the newly created hall to any extra locations

    // Created with a pathfinder
    public List<HallGridPosition> HallwayGridPositions; 

    public void Merge(Hallway hallwayToMergeWith)
    {
        MultiConnectionHall = true;

        MultiConnectionPointList = new Dictionary<int, Vector2Int>
        {
            { fromRoomNumber, From },
            { toRoomNumber, To },
            { hallwayToMergeWith.toRoomNumber, hallwayToMergeWith.To }
        };
        
    }

    public override string ToString()
    {
        if(MultiConnectionHall)
        {
            string multiConnectionString = "";
            foreach(var value in MultiConnectionPointList)
            {
                multiConnectionString += "Room Number: " + value.Key.ToString() + " Position: " + value.Value + " ";
            }
            return $"Multi-Connection Hallway ({multiConnectionString}) Number of Positions in the Grid: {HallwayGridPositions.Count}";
        }
        else
            return $"From Room Number: {fromRoomNumber} Position: {From} To Room Number: {toRoomNumber} Position: {To} Number of Positions in the Grid: {HallwayGridPositions.Count}";
    }
}
