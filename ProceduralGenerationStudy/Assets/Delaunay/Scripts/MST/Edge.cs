using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinSpanTree
{
    /// <summary>
    /// An Edge is what connects nodes in a Minimum Spanning Tree. It does not represent the edge of a room, rather, the edge of the shape made through connecting the node positions.
    /// </summary>
    /// <typeparam name="T"></typeparam> <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Edge<T> 
    { 
        public Node<T> From { get; set; } 
        public Node<T> To { get; set; } 
        public int Weight { get; set; } 

        public void WeighVector2Edge(float maxHallDistance, Vector2 from, Vector2 to)
        {
            float distance = Vector2.Distance(from, to);
            float normalizedDistance = distance / maxHallDistance;
            int edgeWeight = (int)((1 - normalizedDistance) * 10);
            Weight += edgeWeight;
        }

        public override string ToString() 
        { 
            return $"Edge: {From.Data} -> {To.Data},  weight: {Weight}"; 
        }

    } 
}
