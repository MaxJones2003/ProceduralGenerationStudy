using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using node_id = System.UInt32;

namespace QT
{
    public struct QuadTree<T> where T : notnull
    {
        public Box<T> BBox { get; set; }
        public node_id Root { get; set; }
        public List<Node<T>> Nodes { get; set; }

        public node_id Build(IEnumerable<Point<T>> points)
        {
            BBox = Box<T>.BBox(points);
            Root = BuildImpl(this, BBox, points.ToList(), 0, points.Count());
            return Root;
        }

        private node_id BuildImpl(QuadTree<T> tree, Box<T> bbox, List<Point<T>> points, int begin, int end)
        {
            if(begin == end)
            {
                return Node<T>.Null; // return null node ID
            }

            node_id result = (node_id)tree.Nodes.Count();
            tree.Nodes.Add(new Node<T>(points[begin])); // Add a new node

            // If only one point, return the node as a leaf
            if(end - begin == 1)
            {
                return result;
            }

            Point<T> center = Point<T>.Middle(bbox.Min, bbox.Max);

            // Partition points into quadrants
            int splitY = Partition(points, begin, end, p => p.Y < center.Y);
            int splitXLower = Partition(points, begin, splitY, p => p.X < center.X);
            int splitXUpper = Partition(points, splitY, end, p => p.X < center.X);

            // Recursively build child nodes for each quadrant
            tree.Nodes[(int)result].Children[0, 0] = BuildImpl(tree, new Box<T>(bbox.Min, center), points, begin, splitXLower);
            tree.Nodes[(int)result].Children[0, 1] = BuildImpl(tree, new Box<T>(new Point<T>(center.Value, center.X, bbox.Min.Y), new Point<T>(center.Value, bbox.Max.X, center.Y)), points, splitXLower, splitY);
            tree.Nodes[(int)result].Children[1, 0] = BuildImpl(tree, new Box<T>(new Point<T>(center.Value, bbox.Min.X, center.Y), new Point<T>(center.Value, center.X, bbox.Max.Y)), points, splitY, splitXUpper);
            tree.Nodes[(int)result].Children[1, 1] = BuildImpl(tree, new Box<T>(center, bbox.Max), points, splitXUpper, end);

            return result;
        }

        private int Partition(List<Point<T>> points, int begin, int end, Func<Point<T>, bool> predicate)
        {
            int i = begin;
            for (int j = begin; j < end; j++)
            {
                if(predicate(points[j]))
                {
                    (points[j], points[i]) = (points[i], points[j]); // Swap values with a tuple
                    i++;
                }
            }
            return i;
        }
    }
}
