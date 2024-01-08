using System;
using System.Collections.Generic;
using System.Linq;

public class KDTree<T> where T : IKDTreeItem
{
    private Node root;

    public void Insert(List<T> points)
    {
        root = InsertRecursive(points.OrderBy(p => p[0]).ToList(), 0);
    }

    private Node InsertRecursive(List<T> points, int depth)
    {
        if (!points.Any())
        {
            return null;
        }

        int currentDimension = depth % 2;
        int medianIndex = points.Count / 2;

        // Sort the points along the current dimension and choose the median point
        points = points.OrderBy(p => p[currentDimension]).ToList();
        T medianPoint = points[medianIndex];

        // Create a new node with the median point
        Node node = new Node(medianPoint);

        // Recursively insert the points less than the median point into the left subtree
        node.Left = InsertRecursive(points.Take(medianIndex).ToList(), depth + 1);

        // Recursively insert the points greater than the median point into the right subtree
        node.Right = InsertRecursive(points.Skip(medianIndex + 1).ToList(), depth + 1);

        return node;
    }
    public T FindNearestNeighbor(Vector2f queryPoint)
    {
        if (root == null)
        {
            throw new InvalidOperationException("The KD tree is empty.");
        }

        Node nearestNeighbor = FindNearestNeighborRecursive(root, queryPoint, 0);
        return nearestNeighbor.Value;
    }

    private Node FindNearestNeighborRecursive(Node node, Vector2f queryPoint, int depth)
    {
        if (node == null)
        {
            return null;
        }

        int currentDimension = depth % 2;

        Node bestNode = node;
        Node otherNode = null;

        if (queryPoint[currentDimension] < node.Value.Position[currentDimension])
        {
            if (node.Left != null)
            {
                otherNode = node.Left;
            }
        }
        else
        {
            if (node.Right != null)
            {
                otherNode = node.Right;
            }
        }

        Node alternateNode = FindNearestNeighborRecursive(otherNode, queryPoint, depth + 1);
        if (alternateNode != null)
        {
            if (Distance(queryPoint, alternateNode.Value.Position) < Distance(queryPoint, bestNode.Value.Position))
            {
                bestNode = alternateNode;
            }
        }

        // Check the other subtree if the distance to the decision boundary is less than the distance to the current best neighbor
        float distanceToDecisionBoundary = Math.Abs(node.Value.Position[currentDimension] - queryPoint[currentDimension]);
        if (distanceToDecisionBoundary < Distance(queryPoint, bestNode.Value.Position))
        {
            Node alternateNode2 = FindNearestNeighborRecursive(node.Left == otherNode ? node.Right : node.Left, queryPoint, depth + 1);
            if (alternateNode2 != null)
            {
                if (Distance(queryPoint, alternateNode2.Value.Position) < Distance(queryPoint, bestNode.Value.Position))
                {
                    bestNode = alternateNode2;
                }
            }
        }

        return bestNode;
    }

    public T[] FindNearestNeighbors(Vector2f queryPoint, int nearestCount, ref int treversed)
    {
        if (root == null)
        {
            throw new InvalidOperationException("The KD tree is empty.");
        }
        SortedList<float, Node> nearestNodes = new SortedList<float, Node>(new DuplicateKeyComparer<float>());
        FindNearestNeighborsRecursive(root, queryPoint, 0, nearestCount, ref treversed, ref nearestNodes);
        return nearestNodes.Values.Select(n => n.Value).ToArray();
    }
    private void FindNearestNeighborsRecursive(Node node, Vector2f queryPoint, int depth, int count, ref int timesTreversed, ref SortedList<float, Node> nearestNodes)
    {
        // If the node is null, we've reached a leaf node, so return
        if (node == null)
        {
            return;
        }

        // Increment the count of traversed nodes
        timesTreversed++;

        // Determine the current dimension (x or y) based on the depth
        int currentDimension = depth % 2;

        Node otherNode = null;

        // If the query point's current dimension is less than the node's point position,
        // set otherNode to the left child node (if it exists)
        if (queryPoint[currentDimension] < node.Value.Position[currentDimension])
        {
            if (node.Left != null)
            {
                otherNode = node.Left;
            }
        }
        // Otherwise, set otherNode to the right child node (if it exists)
        else
        {
            if (node.Right != null)
            {
                otherNode = node.Right;
            }
        }

        // Recursively call the function on the otherNode
        FindNearestNeighborsRecursive(otherNode, queryPoint, depth + 1, count, ref timesTreversed, ref nearestNodes);

        // Calculate the distance from the query point to the current node's point
        float distance = Distance(queryPoint, node.Value.Position);

        // Add the current node to the nearest nodes list, with its distance as the key
        nearestNodes.Add(distance, node);

        // If the list has more than 'count' nodes, remove the furthest one
        if (nearestNodes.Count > count)
        {
            nearestNodes.RemoveAt(nearestNodes.Count - 1);
        }

        // Calculate the distance to the decision boundary
        float distanceToDecisionBoundary = Math.Abs(node.Value.Position[currentDimension] - queryPoint[currentDimension]);

        // If the nearest nodes list isn't full yet, or the distance to the decision boundary is less than the distance to the furthest node in the list,
        // recursively call the function on the other child node of the current node
        if (nearestNodes.Count < count || distanceToDecisionBoundary < nearestNodes.Keys[nearestNodes.Count - 1])
        {
            FindNearestNeighborsRecursive(node.Left == otherNode ? node.Right : node.Left, queryPoint, depth + 1, count, ref timesTreversed, ref nearestNodes);
        }
    }    

    private float Distance(Vector2f point1, Vector2f point2)
    {
        return (point1 - point2).magnitude;
    }

    private class Node
    {
        public T Value { get; }
        public Node Left { get; set; }
        public Node Right { get; set; }

        public Node(T point)
        {
            Value = point;
        }
    }
}