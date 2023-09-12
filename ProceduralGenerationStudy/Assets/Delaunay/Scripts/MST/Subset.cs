using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinSpanTree
{
    public class Subset<T> 
    { 
        public Node<T> Parent { get; set; } 
        public int Rank { get; set; } 
    
        public override string ToString() 
        { 
            return $"Subset with rank {Rank}, parent: {Parent.Data} (index: {Parent.Index})"; 
        } 
    } 
}

