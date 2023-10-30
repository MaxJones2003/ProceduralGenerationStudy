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
                borders[0] = edges.Find(e => e.Q == face.Q && e.R+1 == face.R && e.Direction == Direction.West);
                borders[1] = edges.Find(e => e.Q+1 == face.Q && e.R == face.R && e.Direction == Direction.North);
                face.borders = borders;
            }
        }

        private void SetUpFaceNeighbors()
        {
            foreach(Face face in faces)
            {
                Face[] neighbors = new Face[4];

                // If any face that is not the current face has a border that is the same as the current face's border, then it is a neighbor
                // If any face does not have a a neighbor, that borders isGridBounds = true
                if(faces.Any(f => f.borders.Any(b => b == face.borders[0]) && f != face))
                    neighbors[0] = faces.Find(f => f.borders.Any(b => b == face.borders[0]) && f != face);
                else
                {
                    face.borders[0].isGridBounds = true;
                    neighbors[0] = new Face(face.Q - 1, face.R, null)
                    {
                        isOutOfGrid = true
                    };
                } 
                if(faces.Any(f => f.borders.Any(b => b == face.borders[1]) && f != face))
                    neighbors[1] = faces.Find(f => f.borders.Any(b => b == face.borders[1]) && f != face);
                else
                {
                    face.borders[1].isGridBounds = true;
                    neighbors[1] = new Face(face.Q, face.R - 1, null)
                    {
                        isOutOfGrid = true
                    };
                }
                if(faces.Any(f => f.borders.Any(b => b == face.borders[2]) && f != face))
                    neighbors[2] = faces.Find(f => f.borders.Any(b => b == face.borders[2]) && f != face);
                else
                {
                    face.borders[2].isGridBounds = true;
                    neighbors[2] = new Face(face.Q + 1, face.R, null)
                    {
                        isOutOfGrid = true
                    };
                }
                if(faces.Any(f => f.borders.Any(b => b == face.borders[3]) && f != face))
                    neighbors[3] = faces.Find(f => f.borders.Any(b => b == face.borders[3]) && f != face);
                else 
                {
                    face.borders[3].isGridBounds = true;
                    neighbors[3] = new Face(face.Q, face.R + 1, null)
                    {
                        isOutOfGrid = true
                    };
                }


                face.neighbors = neighbors;
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
                    edges.Add(new Edge(vertex, vertices.Find(v => v.Q == vertex.Q + 1 && v.R == vertex.R), Direction.West, vertex.Q, vertex.R));
                }
                if(vertices.Any(v => v.Q == vertex.Q && v.R == vertex.R + 1))
                {
                    edges.Add(new Edge(vertex, vertices.Find(v => v.Q == vertex.Q && v.R == vertex.R + 1), Direction.North, vertex.Q, vertex.R));
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
                    vertices.Find(v => v.Q == vertex.Q + 1 && v.R == vertex.R), 
                    vertices.Find(v => v.Q == vertex.Q && v.R == vertex.R + 1), 
                    vertices.Find(v => v.Q == vertex.Q + 1 && v.R == vertex.R + 1)}));
                }
            }
        }



    }
}
