using System;
using System.Collections.Generic;
using System.Linq;
using Map;

public class KDTree
{
    private Node root;

    public void Insert(List<Corner> points)
    {
        root = InsertRecursive(points.OrderBy(p => p.point.x).ToList(),  0);
    }

    private Node InsertRecursive(List<Corner> points, int depth)
    {
        if (!points.Any())
        {
            return null;
        }

        int currentDimension = depth %  2;
        int medianIndex = points.Count /  2;

        points = points.OrderBy(p => p[currentDimension]).ToList();
        Vector2f medianPoint = points[medianIndex].point;

        Node node = new Node(points[medianIndex].index, medianPoint)
        {
            Left = InsertRecursive(points.Take(medianIndex).ToList(), depth + 1),
            Right = InsertRecursive(points.Skip(medianIndex + 1).ToList(), depth + 1)
        };

        return node;
    }

public int FindNearestNeighbor(Vector2f queryPoint)
    {
        if (root == null)
        {
            throw new InvalidOperationException("The KD tree is empty.");
        }

        // Create a stack for the nodes to visit
        var nodesToVisit = new Stack<(Node, int)>();

        // Start with the root node and dimension 0
        nodesToVisit.Push((root, 0));

        Node bestNode = null;
        float bestDistance = float.MaxValue;

        while (nodesToVisit.Count > 0)
        {
            var (currentNode, currentDimension) = nodesToVisit.Pop();

            if (currentNode != null)
            {
                // Calculate the distance from the query point to the current node's point
                float distance = Distance(queryPoint, currentNode.Value);

                // If the current node is closer than the best node
                if (distance < bestDistance)
                {
                    // Update the best node and the best distance
                    bestNode = currentNode;
                    bestDistance = distance;
                }

                // Determine which child node to visit next
                Node nextNode = queryPoint[currentDimension] < currentNode.Value[currentDimension] ? currentNode.Left : currentNode.Right;
                Node otherNode = nextNode == currentNode.Left ? currentNode.Right : currentNode.Left;

                // Add the next node to the stack with the next dimension
                nodesToVisit.Push((nextNode, (currentDimension + 1) % 2));

                // If the distance to the decision boundary is less than the best distance
                if (Math.Abs(currentNode.Value[currentDimension] - queryPoint[currentDimension]) < bestDistance)
                {
                    // Add the other node to the stack with the next dimension
                    nodesToVisit.Push((otherNode, (currentDimension + 1) % 2));
                }
            }
        }

        return bestNode.Index;
    }


    public int[] FindNearestNeighbors(Vector2f queryPoint, int nearestCount)
    {
        if (root == null)
        {
            throw new InvalidOperationException("The KD tree is empty.");
        }

        // Initialize the nearestNodes list with the exact capacity
        var nearestNodes = new SortedList<float, Node>(nearestCount);

        FindNearestNeighborsRecursive(root, queryPoint, 0, nearestCount, nearestNodes);

        // Convert the SortedList of Node objects to an array of Node.Value objects and return it
        return nearestNodes.Values.Select(node => node.Index).ToArray();
    }

    private void FindNearestNeighborsRecursive(Node node, Vector2f queryPoint, int depth, int count, SortedList<float, Node> nearestNodes)
    {
        if (node == null) return;

        int currentDimension = depth % 2;
        float distance = Distance(queryPoint, node.Value);

        // Track and update the best distance and node so far
        if (nearestNodes.Count < count || distance < nearestNodes.Keys[nearestNodes.Count - 1])
        {
            nearestNodes.Add(distance, node);
            if (nearestNodes.Count > count)
            {
                nearestNodes.RemoveAt(nearestNodes.Count - 1);
            }
        }

        Node nextNode = queryPoint[currentDimension] < node.Value[currentDimension] ? node.Left : node.Right;
        Node otherNode = nextNode == node.Left ? node.Right : node.Left;

        // Search down the tree
        FindNearestNeighborsRecursive(nextNode, queryPoint, depth + 1, count, nearestNodes);

        // Check if we need to search the other side
        float distanceToDecisionBoundary = Math.Abs(node.Value[currentDimension] - queryPoint[currentDimension]);
        if (nearestNodes.Count < count || distanceToDecisionBoundary < nearestNodes.Keys[nearestNodes.Count - 1])
        {
            FindNearestNeighborsRecursive(otherNode, queryPoint, depth + 1, count, nearestNodes);
        }
    }

    private float Distance(Vector2f point1, Vector2f point2)
    {
        return (point1 - point2).magnitude;
    }
    public class Node
    {
        public int Index;
        public Vector2f Value { get; }
        public Node Left { get; set; }
        public Node Right { get; set; }

        public Node(int index, Vector2f point)
        {
            Index = index;
            Value = point;
        }
    }
}
