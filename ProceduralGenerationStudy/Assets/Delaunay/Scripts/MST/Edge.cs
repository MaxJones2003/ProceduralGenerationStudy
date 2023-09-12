using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinSpanTree
{
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
