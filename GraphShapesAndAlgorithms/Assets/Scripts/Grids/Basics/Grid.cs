using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geometry
{
    [System.Serializable]
    public class Grid
    {
        public List<Vertex> vertices;
        public List<Edge> edges;
        public List<Face> faces;
        private int width;
        private int height;
        public Grid(int width, int height)
        {
            this.width = width;
            this.height = height;
            vertices = new List<Vertex>(width * height);
            InitializeVertices();
            edges = new List<Edge>(width * height);
            InitializeEdges();
            faces = new List<Face>(width * height);
            InitializeFaces();

            /* SetUpVertices();
            SetUpEdges();*/
            SetUpFaceBorders(); 
            SetUpFaceNeighbors();
        }

        private void SetUpFaceBorders()
        {
            foreach(Face face in faces)
            {
                Edge[] borders = new Edge[4];
                borders[0] = edges.Find(e => e.Q == face.Q && e.R == face.R && e.Direction == Direction.West);
                borders[1] = edges.Find(e => e.Q == face.Q && e.R == face.R && e.Direction == Direction.North);
                borders[2] = edges.Find(e => e.Q == face.Q && e.R == face.R+1 && e.Direction == Direction.West);
                borders[3] = edges.Find(e => e.Q == face.Q+1 && e.R == face.R && e.Direction == Direction.North);
                face.borders = borders;
                Debug.Log("Vertex 1 " + face.borders[0] + " Vertex 2  " + face.borders[1] + " Vertex 3  " + face.borders[2] + " Vertex 4  " + face.borders[3]);
            }
        }

        private void SetUpFaceNeighbors()
        {
            List<Face> neighbors = new List<Face>();
            foreach(Face face in faces)
            {
                neighbors.Clear();
                foreach(Edge border in face.borders)
                {
                    if(faces.Any(f => f.borders.Contains(border) && f != face))
                        neighbors.Add(faces.Find(f => f.borders.Contains(border) && f != face));
                    
                }
                face.neighbors = neighbors.ToArray();
            }
        }

        private void SetUpEdges()
        {
            // Find the joins and continues for each edge
            foreach(Edge edge in edges)
            {
                edge.joins = faces.FindAll(f => f.borders.Contains(edge)).ToArray();
                edge.continues = edges.FindAll(e => e.endPoints.Contains(edge.endPoints[0]) || e.endPoints.Contains(edge.endPoints[1])).ToArray();
            }
        }

        private void SetUpVertices()
        {
            // Find the touches, protrudes, and adjacent vertices for each vertex
            foreach(Vertex vertex in vertices)
            {
                vertex.touches = faces.FindAll(f => f.corners.Contains(vertex)).ToArray();
                vertex.protrudes = edges.FindAll(e => e.endPoints.Contains(vertex)).ToArray();
                vertex.adjacent = vertices.FindAll(v => v.protrudes != null && v.protrudes.Length > 0 && v.protrudes[0].endPoints.Contains(vertex)).ToArray();
            }
        }

        // first initialize the grid with all the vertices with its q and r values
        private void InitializeVertices()
        {
            for(int q = 0; q < width; q++)
            {
                for(int r = 0; r < height; r++)
                {
                    vertices.Add(new Vertex(q, r));
                }
            }
        }
        private void InitializeEdges()
        {
            foreach(Vertex vertex in vertices)
            {
                if(vertices.Any(v => v.Q == vertex.Q + 1 && v.R == vertex.R))
                {
                    edges.Add(new Edge(vertex, vertices.Find(v => v.Q == vertex.Q + 1 && v.R == vertex.R), Direction.North, vertex.Q, vertex.R));
                }
                if(vertices.Any(v => v.Q == vertex.Q && v.R == vertex.R + 1))
                {
                    edges.Add(new Edge(vertex, vertices.Find(v => v.Q == vertex.Q && v.R == vertex.R + 1), Direction.West, vertex.Q, vertex.R));
                }
            }
        }

        // Initialize faces, if a vertex has a vertex to the right and below it, then it has a face, when assigning faces to vertices, the q and r values should be the same
        private void InitializeFaces()
        {
            foreach(Vertex vertex in vertices)
            {
                if(vertex.Q + 1 < width && vertex.R + 1 < height)
                {
                    faces.Add(new Face(vertex.Q, vertex.R, 
                    new Vertex[] {vertex, 
                    vertices.Find(v => v.Q == vertex.Q && v.R == vertex.R + 1), 
                    vertices.Find(v => v.Q == vertex.Q + 1 && v.R == vertex.R + 1), 
                    vertices.Find(v => v.Q == vertex.Q + 1 && v.R == vertex.R)}));
                }
            }
        }
    }
}
