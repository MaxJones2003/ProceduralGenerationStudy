using csDelaunay;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map
{
    public class Center : IKDTreeItem
    {
        //public Center(Vector2f point, Site site)
        //{
        //    this.point = point;
        //    sites = new();
        //    sites.Add(site);
        //     neighbors = new List<Center>();
        //    borders = new List<Edge>();
        //    corners = new List<Corner>(); 
        //}

        public int index;

        public Vector2f point;  // location
        
        [HideInInspector]public List<Site> sites; // Any Delaunay cell with this point in its edges

        #region IKDTreeItem
        public Vector2f Position => point;

        public float this[int dimension] => point[dimension];
        #endregion

        public bool water;  // lake or ocean
        public bool ocean;  // ocean
        public bool coast;  // land polygon touching an ocean
        public bool border;  // at the edge of the map
        public string biome;  // biome type (see article)
        public EBiomeType biomeEnum;
        public float elevation;  // 0.0-1.0
        public float moisture;  // 0.0-1.0

        [HideInInspector] public List<Center> neighbors;
        [HideInInspector] public List<Edge> borders;
        [HideInInspector] public List<Corner> corners;

        public void OrderCornersClockwise()
        {
            if (corners.Count == 0)
                return;

            // set the reference point to the left most corner
            Vector2f referencePoint = corners.OrderBy(c => c.point.y).ThenBy(c => c.point.x).First().point;

            corners.Sort((a, b) =>
            {
                float angleA = Mathf.Atan2(a.point.y - referencePoint.y, a.point.x - referencePoint.x);
                float angleB = Mathf.Atan2(b.point.y - referencePoint.y, b.point.x - referencePoint.x);

                // Adjust the angles to be between 0 and 2π
                angleA = (angleA + 2 * Mathf.PI) % (2 * Mathf.PI);
                angleB = (angleB + 2 * Mathf.PI) % (2 * Mathf.PI);

                // Reverse the comparison to sort in clockwise order
                return angleB.CompareTo(angleA);
            });
        }
        /* public void OrderCornersClockwise()
        {
            Vector2f centroid = CalculateCentroid();

            corners.Sort((a, b) =>
            {
                float angleA = Mathf.Atan2(a.point.y - centroid.y, a.point.x - centroid.x);
                float angleB = Mathf.Atan2(b.point.y - centroid.y, b.point.x - centroid.x);

                // Adjust the angles to be between 0 and 2π
                angleA = (angleA + 2 * Mathf.PI) % (2 * Mathf.PI);
                angleB = (angleB + 2 * Mathf.PI) % (2 * Mathf.PI);

                // Reverse the comparison to sort in clockwise order
                return angleB.CompareTo(angleA);
            });
        } */

        private Vector2f CalculateCentroid()
        {
            Vector2f centroid = Vector2f.zero;

            foreach (Corner corner in corners)
            {
                centroid += corner.point;
            }

            return centroid / corners.Count;
        }

        public bool Inside(Vector2f P)
        {
            //OrderCornersClockwise();
            bool result = false;
            int j = corners.Count - 1;
            for (int i = 0; i < corners.Count; i++)
            {
                if (corners[i].point.y < P.y && corners[j].point.y >= P.y || 
                    corners[j].point.y < P.y && corners[i].point.y >= P.y)
                {
                    if (corners[i].point.x + (P.y - corners[i].point.y) /
                    (corners[j].point.y - corners[i].point.y) *
                    (corners[j].point.x - corners[i].point.x) < P.x)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        #region Overrides
        // Override comparison for a Vector2f and a Center, where the Vector2f is the point of the center
        public static bool operator ==(Center c, Vector2f v)
        {
            return c.point == v;
        }

        // Define the != operator as well
        public static bool operator !=(Center c, Vector2f v)
        {
            return !(c == v);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

    }
}
