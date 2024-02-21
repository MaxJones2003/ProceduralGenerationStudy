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
                float distance = Distance(queryPoint, currentNode.Value.Position);

                // If the current node is closer than the best node
                if (distance < bestDistance)
                {
                    // Update the best node and the best distance
                    bestNode = currentNode;
                    bestDistance = distance;
                }

                // Determine which child node to visit next
                Node nextNode = queryPoint[currentDimension] < currentNode.Value.Position[currentDimension] ? currentNode.Left : currentNode.Right;
                Node otherNode = nextNode == currentNode.Left ? currentNode.Right : currentNode.Left;

                // Add the next node to the stack with the next dimension
                nodesToVisit.Push((nextNode, (currentDimension + 1) % 2));

                // If the distance to the decision boundary is less than the best distance
                if (Math.Abs(currentNode.Value.Position[currentDimension] - queryPoint[currentDimension]) < bestDistance)
                {
                    // Add the other node to the stack with the next dimension
                    nodesToVisit.Push((otherNode, (currentDimension + 1) % 2));
                }
            }
        }

        return bestNode.Value;
    }

    /* public T[] FindNearestNeighbors(Vector2f queryPoint, int nearestCount)
    {
        if (root == null)
        {
            throw new InvalidOperationException("The KD tree is empty.");
        }
        SortedList<float, Node> nearestNodes = new SortedList<float, Node>(new DuplicateKeyComparer<float>());
        FindNearestNeighborsRecursive(root, queryPoint, 0, nearestCount, ref nearestNodes);
        return nearestNodes.Values.Select(n => n.Value).ToArray();
    } */


    /* private void FindNearestNeighborsRecursive(Node node, Vector2f queryPoint, int depth, int count,  ref SortedList<float, Node> nearestNodes)
    {
        // If the node is null, we've reached a leaf node, so return
        if (node == null)
        {
            return;
        }

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
        FindNearestNeighborsRecursive(otherNode, queryPoint, depth + 1, count, ref nearestNodes);

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
            FindNearestNeighborsRecursive(node.Left == otherNode ? node.Right : node.Left, queryPoint, depth + 1, count, ref nearestNodes);
        }
    }     */
    /* private void FindNearestNeighborsRecursive(Node node, Vector2f queryPoint, int depth, int count, ref SortedList<float, Node> nearestNodes)
    {
        if (node == null) return;

        int currentDimension = depth % 2;
        float distance = Distance(queryPoint, node.Value.Position);

        // Track and update the best distance and node so far
        if (nearestNodes.Count < count || distance < nearestNodes.Keys[nearestNodes.Count - 1])
        {
            nearestNodes.Add(distance, node);
            if (nearestNodes.Count > count)
            {
                nearestNodes.RemoveAt(nearestNodes.Count - 1);
            }
        }

        Node nextNode = queryPoint[currentDimension] < node.Value.Position[currentDimension] ? node.Left : node.Right;
        Node otherNode = nextNode == node.Left ? node.Right : node.Left;

        // Search down the tree
        FindNearestNeighborsRecursive(nextNode, queryPoint, depth + 1, count, ref nearestNodes);

        // Check if we need to search the other side
        float distanceToDecisionBoundary = Math.Abs(node.Value.Position[currentDimension] - queryPoint[currentDimension]);
        if (nearestNodes.Count < count || distanceToDecisionBoundary < nearestNodes.Keys[nearestNodes.Count - 1])
        {
            FindNearestNeighborsRecursive(otherNode, queryPoint, depth + 1, count, ref nearestNodes);
        }
    } */

    public T[] FindNearestNeighbors(Vector2f queryPoint, int nearestCount)
    {
        if (root == null)
        {
            throw new InvalidOperationException("The KD tree is empty.");
        }

        // Initialize the nearestNodes list with the exact capacity
        var nearestNodes = new SortedList<float, Node>(nearestCount);

        FindNearestNeighborsRecursive(root, queryPoint, 0, nearestCount, nearestNodes);

        // Convert the SortedList of Node objects to an array of Node.Value objects and return it
        return nearestNodes.Values.Select(node => node.Value).ToArray();
    }

    private void FindNearestNeighborsRecursive(Node node, Vector2f queryPoint, int depth, int count, SortedList<float, Node> nearestNodes)
    {
        if (node == null) return;

        int currentDimension = depth % 2;
        float distance = Distance(queryPoint, node.Value.Position);

        // Track and update the best distance and node so far
        if (nearestNodes.Count < count || distance < nearestNodes.Keys[nearestNodes.Count - 1])
        {
            nearestNodes.Add(distance, node);
            if (nearestNodes.Count > count)
            {
                nearestNodes.RemoveAt(nearestNodes.Count - 1);
            }
        }

        Node nextNode = queryPoint[currentDimension] < node.Value.Position[currentDimension] ? node.Left : node.Right;
        Node otherNode = nextNode == node.Left ? node.Right : node.Left;

        // Search down the tree
        FindNearestNeighborsRecursive(nextNode, queryPoint, depth + 1, count, nearestNodes);

        // Check if we need to search the other side
        float distanceToDecisionBoundary = Math.Abs(node.Value.Position[currentDimension] - queryPoint[currentDimension]);
        if (nearestNodes.Count < count || distanceToDecisionBoundary < nearestNodes.Keys[nearestNodes.Count - 1])
        {
            FindNearestNeighborsRecursive(otherNode, queryPoint, depth + 1, count, nearestNodes);
        }
    }

    private float Distance(Vector2f point1, Vector2f point2)
    {
        return (point1 - point2).sqrMagnitude;
    }
    public class Node
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


public interface IKDTreeItem
{
    Vector2f Position { get; }
    float this[int dimension] { get; }
}

public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1;   // Handle equality as being greater
        else
            return result;
    }
}
