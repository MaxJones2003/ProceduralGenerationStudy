using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public struct Grid
    {
        public int mapSize;
        public float gridspaceSize;
        public Box[,] gridSpaces;
        public Grid(int mapSize, List<Center> centers)
        {
            this.mapSize = mapSize;
            gridspaceSize = mapSize / 10;
            
            gridSpaces = CreateGrid(mapSize);
            OrganizeCenters(centers);
        }

        public static Box[,] CreateGrid(int mapSize)
        {
            int boxSize = mapSize / 10;
            Box[,] boxes = new Box[10, 10];
            for (int x = boxSize/2; x < mapSize; x += boxSize)
            {
                int xIndex = x == 0 ? 0 : x / 100;
                for (int y = boxSize/2; y < mapSize; y += boxSize)
                {
                    int yIndex = y == 0 ? 0 : y / 100;
                    boxes[xIndex, yIndex] = new Box(new Vector2f(x, y), boxSize);
                }
            }
            return boxes;
        }
        public void OrganizeCenters(List<Center> centers)
        {
            foreach(var center in centers)
            {
                Vector2f centerPoint = center.point / 100;
                // round so that if the value is 0-99 it will be 0, 100-199 will be 1, etc.
                int x = (int)Mathf.Floor(centerPoint.x);
                int y = (int)Mathf.Floor(centerPoint.y);

                gridSpaces[x, y].centers.Add(center);
                List<Vector2f> cordinatesAdded = new List<Vector2f>();
                cordinatesAdded.Add(new Vector2f(x, y));
                foreach(var corner in center.corners)
                {
                    Vector2f cornerPoint = corner.point / 100;
                    x = (int)Mathf.Floor(centerPoint.x);
                    y = (int)Mathf.Floor(centerPoint.y);
                    if(!cordinatesAdded.Contains(new Vector2f(x, y)))
                    {
                        gridSpaces[x, y].centers.Add(center);
                        cordinatesAdded.Add(new Vector2f(x, y));
                    }
                    //UnityEngine.Debug.Log(cordinatesAdded.Count + " " + x + ", " + y);
                }
            }
        }

        public List<Center> FindCentersInGrid(int x, int y)
        {
            x = x/100;
            y = y/100;

            return gridSpaces[x, y].centers;
        }

        public class Box
        {
            public Vector2f center;
            public Vector2f[] boxCorners;
            public List<Center> centers;

            public Box(Vector2f center, float size)
            {
                this.center = center;
                boxCorners = new Vector2f[4] {
                    new Vector2f(center.x - size / 2, center.y - size / 2),
                    new Vector2f(center.x + size / 2, center.y - size / 2),
                    new Vector2f(center.x + size / 2, center.y + size / 2),
                    new Vector2f(center.x - size / 2, center.y + size / 2)
                };
                centers = new List<Center>();
            }
        }
    }
}