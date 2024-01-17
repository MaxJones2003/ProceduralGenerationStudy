using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using node_id = System.UInt32;
// https://doc.embedded-wizard.de/uint-type
// https://lisyarus.github.io/blog/programming/2022/12/21/quadtrees.html
// https://stopbyte.com/t/whats-the-equivalent-of-c-uint8-t-type-in-c/335/2
namespace QT
{
    public struct Node<T> where T : notnull
    {
        public static readonly uint Null = 0xFFFFFFFF; // Equivalent to node_id(-1) in C++
        public node_id? Parent { get; set; }
        public node_id?[,] Children { get; set; }
        public Point<T> Data { get; set; }

        public Node(Point<T> data)
        {
            Parent = null;
            Children = new node_id?[2, 2]; // Assuming a 2D quadtree with 4 children
            Data = data;
        }
    }
}
