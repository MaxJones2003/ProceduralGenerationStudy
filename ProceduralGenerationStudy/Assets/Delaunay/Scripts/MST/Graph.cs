using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinSpanTree
{
    public class Graph<T> 
    { 
        #region GraphCode
        private bool _isDirected = false; 
        private bool _isWeighted = false; 
        public List<Node<T>> Nodes { get; set; }  
            = new List<Node<T>>(); 
        
        // Constructor
        public Graph(bool isDirected, bool isWeighted) 
        { 
            _isDirected = isDirected; 
            _isWeighted = isWeighted; 
        } 

        // Indexer
        public Edge<T> this[int from, int to] 
        { 
            get 
            { 
                Node<T> nodeFrom = Nodes[from]; 
                Node<T> nodeTo = Nodes[to]; 
                int i = nodeFrom.Neighbors.IndexOf(nodeTo); 
                if (i >= 0) 
                { 
                    Edge<T> edge = new Edge<T>() 
                    { 
                        From = nodeFrom, 
                        To = nodeTo, 
                        Weight = i < nodeFrom.Weights.Count  
                            ? nodeFrom.Weights[i] : 0 
                    }; 
                    return edge; 
                } 
        
                return null; 
            } 
        } 

        // Add a new node instance
        public Node<T> AddNode(T value) 
        { 
            Node<T> node = new Node<T>() { Data = value }; 
            Nodes.Add(node); 
            UpdateIndices(); 
            return node; 
        } 

        // Remove a current node
        public void RemoveNode(Node<T> nodeToRemove) 
        { 
            Nodes.Remove(nodeToRemove); 
            UpdateIndices(); 
            foreach (Node<T> node in Nodes) 
            { 
                RemoveEdge(node, nodeToRemove); 
            } 
        } 

        // Add a new edge instance using two current nodes
        public void AddEdge(Node<T> from, Node<T> to, int weight = 0) 
        { 
            from.Neighbors.Add(to); 
            if (_isWeighted) 
            { 
                from.Weights.Add(weight); 
            } 
        
            if (!_isDirected) 
            { 
                to.Neighbors.Add(from); 
                if (_isWeighted) 
                { 
                    to.Weights.Add(weight); 
                } 
            } 
        } 

        // Remove a current edge
        public void RemoveEdge(Node<T> from, Node<T> to) 
        { 
            int index = from.Neighbors.FindIndex(n => n == to); 
            if (index >= 0) 
            { 
                from.Neighbors.RemoveAt(index);
                if (_isWeighted)
                { 
                    from.Weights.RemoveAt(index); 
                }
            } 
        } 

        // Gets a collection of all edges available in the graph
        public List<Edge<T>> GetEdges() 
        { 
            List<Edge<T>> edges = new List<Edge<T>>(); 
            foreach (Node<T> from in Nodes) 
            { 
                for (int i = 0; i < from.Neighbors.Count; i++) 
                { 
                    Edge<T> edge = new Edge<T>() 
                    { 
                        From = from, 
                        To = from.Neighbors[i], 
                        Weight = i < from.Weights.Count  
                            ? from.Weights[i] : 0 
                    }; 
                    edges.Add(edge); 
                } 
            } 
            return edges; 
        } 
        
        // Used to update the index value of nodes and edges when one is removed
        private void UpdateIndices() 
        { 
            int i = 0; 
            Nodes.ForEach(n => n.Index = i++); 
        } 
        #endregion
        #region  Algorithms

        // Depth-First search algorithm
        public List<Node<T>> DFS() 
        { 
            bool[] isVisited = new bool[Nodes.Count]; 
            List<Node<T>> result = new List<Node<T>>(); 
            DFS(isVisited, Nodes[0], result); 
            return result; 
        } 

        // Depth-First search variant
        private void DFS(bool[] isVisited, Node<T> node, List<Node<T>> result) 
        { 
            result.Add(node); 
            isVisited[node.Index] = true; 
        
            foreach (Node<T> neighbor in node.Neighbors) 
            { 
                if (!isVisited[neighbor.Index]) 
                { 
                    DFS(isVisited, neighbor, result); 
                } 
            } 
        } 

        // Breadth-First search
        public List<Node<T>> BFS() 
        { 
            return BFS(Nodes[0]); 
        } 

        private List<Node<T>> BFS(Node<T> node) 
        { 
            bool[] isVisited = new bool[Nodes.Count]; 
            isVisited[node.Index] = true; 
        
            List<Node<T>> result = new List<Node<T>>(); 
            Queue<Node<T>> queue = new Queue<Node<T>>(); 
            queue.Enqueue(node); 
            while (queue.Count > 0) 
            { 
                Node<T> next = queue.Dequeue(); 
                result.Add(next); 
        
                foreach (Node<T> neighbor in next.Neighbors) 
                { 
                    if (!isVisited[neighbor.Index]) 
                    { 
                        isVisited[neighbor.Index] = true; 
                        queue.Enqueue(neighbor); 
                    } 
                } 
            } 
        
            return result; 
        } 

        public List<Edge<T>> MinimumSpanningTreeKruskal() 
        { 
            List<Edge<T>> edges = GetEdges(); 
            edges.Sort((a, b) => a.Weight.CompareTo(b.Weight));
            Queue<Edge<T>> queue = new Queue<Edge<T>>(edges); 
        
            Subset<T>[] subsets = new Subset<T>[Nodes.Count]; 
            for (int i = 0; i < Nodes.Count; i++) 
            { 
                subsets[i] = new Subset<T>() { Parent = Nodes[i] }; 
            } 
        
            List<Edge<T>> result = new List<Edge<T>>(); 
            while (result.Count < Nodes.Count - 1) 
            { 
                Edge<T> edge = queue.Dequeue(); 
                Node<T> from = GetRoot(subsets, edge.From); 
                Node<T> to = GetRoot(subsets, edge.To); 
                if (from != to) 
                { 
                    result.Add(edge); 
                    Union(subsets, from, to); 
                } 
            } 
        
            return result; 
        } 

        private Node<T> GetRoot(Subset<T>[] subsets, Node<T> node) 
        { 
            if (subsets[node.Index].Parent != node) 
            { 
                subsets[node.Index].Parent = GetRoot( 
                    subsets, 
                    subsets[node.Index].Parent); 
            } 
        
            return subsets[node.Index].Parent; 
        } 

        private void Union(Subset<T>[] subsets, Node<T> a, Node<T> b) 
        { 
            if (subsets[a.Index].Rank > subsets[b.Index].Rank) 
            { 
                subsets[b.Index].Parent = a; 
            } 
            else if (subsets[a.Index].Rank < subsets[b.Index].Rank) 
            { 
                subsets[a.Index].Parent = b; 
            } 
            else 
            { 
                subsets[b.Index].Parent = a; 
                subsets[a.Index].Rank++; 
            } 
        } 
        #endregion
    } 

    
}
