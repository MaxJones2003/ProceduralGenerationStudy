using Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace NewKDTree
{
    public class KDTree
    {
        private class Node
        {
            public Corner Corner;
            public Node Left;
            public Node Right;

            public Node(Corner corner)
            {
                Corner = corner;
            }
        }

        private Node root;

        public KDTree(List<Corner> corners)
        {
            root = BuildTree(corners,  0);
        }

        private Node BuildTree(List<Corner> corners, int depth)
        {
            if (corners.Count ==  0) return null;

            int axis = depth % 2;
            corners.Sort((a, b) => a.point[axis].CompareTo(b.point[axis]));
            int median = corners.Count /  2;

            Node node = new Node(corners[median]);
            node.Left = BuildTree(corners.GetRange(0, median), depth +  1);
            node.Right = BuildTree(corners.GetRange(median +  1, corners.Count - median -  1), depth +  1);

            return node;
        }

        public Corner NearestNeighbor(Vector2f queryPoint)
        {
            return NearestNeighbor(root, queryPoint,  0, null, double.MaxValue).Corner;
        }

        private Node NearestNeighbor(Node node, Vector2f queryPoint, int depth, Node best, double bestDistance)
        {
            if (node == null) return best;

            int axis = depth % 2;
            double distance = DistanceSquared(queryPoint, node.Corner.point);

            if (distance < bestDistance)
            {
                best = node;
                bestDistance = distance;
            }

            if (queryPoint[axis] < node.Corner.point[axis])
            {
                best = NearestNeighbor(node.Left, queryPoint, depth +  1, best, bestDistance);
                if (Math.Pow(queryPoint[axis] - node.Corner.point[axis],  2) < bestDistance)
                    best = NearestNeighbor(node.Right, queryPoint, depth +  1, best, bestDistance);
            }
            else
            {
                best = NearestNeighbor(node.Right, queryPoint, depth +  1, best, bestDistance);
                if (Math.Pow(queryPoint[axis] - node.Corner.point[axis],  2) < bestDistance)
                    best = NearestNeighbor(node.Left, queryPoint, depth +  1, best, bestDistance);
            }

            return best;
        }

        private double DistanceSquared(Vector2f point1, Vector2f point2)
        {
            return Vector2f.DistanceSquare(point1, point2);
        }

        public List<Corner> NearestNeighbors(Vector2f queryPoint, int x)
        {
            SortedSet<Neighbor> neighbors = new SortedSet<Neighbor>(new NeighborComparer());
            NearestNeighbors(root, queryPoint,  0, neighbors, x);
            return neighbors.Select(n => n.Corner).ToList();
        }

        private void NearestNeighbors(Node node, Vector2f queryPoint, int depth, SortedSet<Neighbor> neighbors, int x)
        {
            if (node == null) return;

            int axis = depth % 2;
            double distance = DistanceSquared(queryPoint, node.Corner.point);

            neighbors.Add(new Neighbor(node.Corner, distance));
            if (neighbors.Count > x)
            {
                neighbors.Remove(neighbors.Max);
            }

            if (queryPoint[axis] < node.Corner.point[axis])
            {
                NearestNeighbors(node.Left, queryPoint, depth +  1, neighbors, x);
                if (neighbors.Count < x || Math.Pow(queryPoint[axis] - node.Corner.point[axis],  2) < neighbors.Max.Distance)
                    NearestNeighbors(node.Right, queryPoint, depth +  1, neighbors, x);
            }
            else
            {
                NearestNeighbors(node.Right, queryPoint, depth +  1, neighbors, x);
                if (neighbors.Count < x || Math.Pow(queryPoint[axis] - node.Corner.point[axis],  2) < neighbors.Max.Distance)
                    NearestNeighbors(node.Left, queryPoint, depth +  1, neighbors, x);
            }
        }
    }

    public class Neighbor
    {
        public Corner Corner { get; set; }
        public double Distance { get; set; }

        public Neighbor(Corner corner, double distance)
        {
            Corner = corner;
            Distance = distance;
        }
    }

    public class NeighborComparer : IComparer<Neighbor>
    {
        public int Compare(Neighbor x, Neighbor y)
        {
            return x.Distance.CompareTo(y.Distance);
        }
    }
}

