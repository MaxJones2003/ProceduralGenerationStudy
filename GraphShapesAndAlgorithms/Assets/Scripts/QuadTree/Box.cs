using System;
using System.Collections.Generic;
using System.Linq;

namespace QT
{
    public struct Box<T> where T : notnull
    {
        public Point<T> Min;
        public Point<T> Max;

        public Box(Point<T> min, Point<T> max)
        {
            Min = min;
            Max = max;
        }

        // Method to update the box with a new point
        public void Include(Point<T> p)
        {
            Min.X = Math.Min(Min.X, p.X);
            Min.Y = Math.Min(Min.Y, p.Y);
            Max.X = Math.Max(Max.X, p.X);
            Max.Y = Math.Max(Max.Y, p.Y);
        }

        // Static method to compute the bounding box of a sequence of points
        public static Box<T> BBox(IEnumerable<Point<T>> points)
        {
            Box<T> result = CreateInfiniteBounds(points.First().Value);
            foreach(var point in points)
            {
                result.Include(point);
            }
            return result;
        }

        // Static constructor to create a Box with 'infinite' bounds
        public static Box<T> CreateInfiniteBounds(T defaultValue)
        {
            var min = new Point<T>(defaultValue, float.PositiveInfinity, float.PositiveInfinity);
            var max = new Point<T>(defaultValue, float.NegativeInfinity, float.NegativeInfinity);
            
            return new Box<T>(min, max);
        }
    }
}