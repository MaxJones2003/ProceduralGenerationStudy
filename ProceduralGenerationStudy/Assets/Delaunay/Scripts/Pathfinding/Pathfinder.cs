using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://github.com/pixelfac/2D-Astar-Pathfinding-in-Unity/blob/master/Pathfinding2D.cs


public class Pathfinder: MonoBehaviour
{
    Dictionary<Vector2Int, bool> cellPositionDictionary = new Dictionary<Vector2Int, bool>();
    public Transform seeker, target;
    public Grid2D grid;
    Node2D seekerNode, targetNode;
    public GameObject GridOwner;

    private void Start()
    {
        grid = GridOwner.GetComponent<Grid2D>();
    }

    public void SetPositionDictionary(Dictionary<Vector2Int, bool> dictionary)
    {
        cellPositionDictionary = dictionary;
    }
    public void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        //get player and target position in grid coords
        seekerNode = grid.NodeFromWorldPoint(startPos);
        targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node2D> openSet = new List<Node2D>();
        HashSet<Node2D> closedSet = new HashSet<Node2D>();
        openSet.Add(seekerNode);
        
        //calculates path for pathfinding
        while (openSet.Count > 0)
        {

            //iterates through openSet and finds lowest FCost
            Node2D node = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost <= node.FCost)
                {
                    if (openSet[i].hCost < node.hCost)
                        node = openSet[i];
                }
            }

            openSet.Remove(node);
            closedSet.Add(node);

            //If target found, retrace path
            if (node == targetNode)
            {
                RetracePath(seekerNode, targetNode);
                return;
            }
            
            //adds neighbor nodes to openSet
            foreach (Node2D neighbour in grid.GetNeighbors(node))
            {
                if (neighbour.obstacle || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = node;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
    }

    //reverses calculated path so first node is closest to seeker
    void RetracePath(Node2D startNode, Node2D endNode)
    {
        List<Node2D> path = new List<Node2D>();
        Node2D currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        grid.path = path;

    }

    //gets distance between 2 nodes for calculating cost
    int GetDistance(Node2D nodeA, Node2D nodeB)
    {
        int dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public void GetAllGridPositions(Vector2Int GridSize, List<Room> rooms)
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


