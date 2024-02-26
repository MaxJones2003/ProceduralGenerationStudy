using Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewKDTree
{
    private Node root;
    private Center[] centers;

    public void Insert(List<Center> centers)
    {
        this.centers = centers.ToArray();
    }
    private Node InsertRecursive(Center[] points, int depth)
    {
        if(!points.Any())
        {
            return null;
        }

        int currentDimension = depth % 2;
        int medianIndex = points.Length / 2;

        points = points.OrderBy(p => p[currentDimension]).ToArray();
        int medianPoint = points[medianIndex].index;

        Node node = new Node(medianPoint);

        node.Left = InsertRecursive(points.Take(medianIndex).ToArray(), depth + 1);
        node.Right = InsertRecursive(points.Skip(medianPoint + 1).ToArray(), depth + 1);

        return node;
    }

    public int[] FindNearestNeighbor(Vector2f queryPoint, int nearestCount)
    {
        if(root == null)
        {
            throw new InvalidOperationException("The KD Tree is empty.");
        }

        var nearestNodes = new SortedList<float, Node>(nearestCount);

        
    }

    private float SqrMagnitude(Vector2f point1, Vector2f point2)
    {
        return (point1 - point2).sqrMagnitude;
    }
}


public class Node
{
    public int Index;
    public Node Left;
    public Node Right;

    public Node(int index)
    {
        Index = index;
    }
}
