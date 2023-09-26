using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Pathfinder : MonoBehaviour
    {
        Dictionary<Vector2Int, bool> cellPositionDictionary = new Dictionary<Vector2Int, bool>();
   

        // Create a function that take in all positions on the grid.
        // If a location on that grid has a room tile, that position is taken
        // https://github.com/pixelfac/2D-Astar-Pathfinding-in-Unity/blob/master/Pathfinding2D.cs
    
        public void GetAllGridPositions(Vector2 GridSize, List<Room> rooms)
        {
            //populate grid dictionary with a nested for loop the x goes to the grid width
            for(int x = 0; x < GridSize.x; x++)
            {
                //the y goes to the grid height
                for(int y = 0; y < GridSize.y; y++)
                {
                    cellPositionDictionary.Add(new Vector2Int(x, y), false);
                }
            }

            //now go through all rooms and their positions
            foreach(Room room in rooms)
            {
                //search the dictionary by those key positions and make the dictionary value true
                foreach(Vector2Int position in room.roomGridPositions)
                {
                    cellPositionDictionary[position] = true;
                }
            }
        }    
    }
}


