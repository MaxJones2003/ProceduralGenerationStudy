using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Map;

public class IDWInterpolator
{
    public List<Corner> corners;
    public float power;
    public IDWInterpolator(List<Corner> corners, float power, int SIZE)
    {
        corners.RemoveAll(c => c.point.x > SIZE || c.point.y > SIZE || c.point.x < 0 || c.point.y < 0);
        this.corners = corners;
        this.power = power;
    }
    public float InverseDistanceWeighting(Vector2f targetPoint)
    {
        float weightedSum = 0.0f;
        float weightSum = 0.0f;

        foreach (var data in corners)
        {
            float distance = Vector2f.Distance(targetPoint, data.point);

            // Avoid division by zero
            if (distance > 0.0001f)
            {
                float weight = 1.0f / Mathf.Pow(distance, power);
                weightedSum += weight * data.elevation;
                weightSum += weight;
            }
        }

        if (weightSum > 0.0f)
            return weightedSum / weightSum;
        else
            return float.NaN; // Handle case where all points are coincident with the target
    }

    public static float InverseDistanceWeighting(Vector2f targetPoint, List<Corner> corners, float power)
    {
        float weightedSum = 0.0f;
        float weightSum = 0.0f;

        foreach (var data in corners)
        {
            float distance = Vector2f.Distance(targetPoint, data.point);

            // Avoid division by zero
            if (distance > 0.0001f)
            {
                float weight = 1.0f / Mathf.Pow(distance, power);
                weightedSum += weight * data.elevation;
                weightSum += weight;
            }
        }

        if (weightSum > 0.0f)
            return weightedSum / weightSum;
        else
            return float.NaN; // Handle case where all points are coincident with the target
    }

    public float Interpolate(Vector2f interpolationPoint)
    {
        float weightedSum = 0;
        float sumOfWeights = 0;

        foreach (Corner corner in corners)
        {
            // Calculate distance to each corner
            float distance = Distance(corner.point, interpolationPoint);

            // Avoid division by zero
            if (distance == 0)
            {
                return corner.elevation; // If the interpolation point coincides with a known point, return its elevation directly
            }

            // Calculate weight using the IDW formula
            float weight = 1.0f / (float)Mathf.Pow(distance, power);

            // Accumulate the weighted sum and sum of weights
            weightedSum += weight * corner.elevation;
            sumOfWeights += weight;
        }

        // Avoid division by zero
        if (sumOfWeights == 0)
        {
            return float.NaN; // Unable to interpolate (all distances are zero)
        }

        // Calculate the final interpolated value
        return weightedSum / sumOfWeights;
    }

    // Function to calculate distance between two Vector2f points
    public static float Distance(Vector2f point1, Vector2f point2)
    {
        float dx = point1.x - point2.x;
        float dy = point1.y - point2.y;
        return (float)Mathf.Sqrt(dx * dx + dy * dy);
    }

    public class PolygonChecker
    {
        public static bool IsPointInsidePolygon(Vector2f point, List<Vector2f> polygonCorners)
        {
            int numCorners = polygonCorners.Count;

            if (numCorners < 3)
            {
                // A polygon must have at least 3 corners
                return false;
            }

            float totalAngle = 0.0f;

            for (int i = 0; i < numCorners; i++)
            {
                Vector2f currentCorner = polygonCorners[i];
                Vector2f nextCorner = polygonCorners[(i + 1) % numCorners]; // Wrap around for the last corner

                Vector2f vector1 = currentCorner - point;
                Vector2f vector2 = nextCorner - point;

                float angle = Vector2.SignedAngle((Vector2)vector1, (Vector2)vector2);
                totalAngle += angle;
            }

            totalAngle = Mathf.Abs(totalAngle);

            return Mathf.Approximately(totalAngle, 2 * Mathf.PI) || Mathf.Approximately(totalAngle, -2 * Mathf.PI);
        }
    }

}

public class KrigingAlgorithm
{
    // Your data points with coordinates and values
    private Vector2f[] dataPoints;
    private float[] values;

    // Kriging parameters
    private float nugget = 0.1f;
    private float range = 1.0f;
    private float sill = 0.9f;
    private int SIZE;
    public KrigingAlgorithm(List<Corner> corners, int SIZE)
    {
        this.SIZE = SIZE;
        // Remove all corners outside of the size using a lamda expression
        corners.RemoveAll(c => c.point.x > SIZE || c.point.y > SIZE || c.point.x < 0 || c.point.y < 0);

        // Set up your semi-random data points and corresponding values
        // This is just an example; replace it with your actual data
        int numberOfPoints = corners.Count;
        dataPoints = new Vector2f[numberOfPoints];
        values = new float[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            dataPoints[i] = corners[i].point;
            values[i] = corners[i].elevation;
        }
    }

    public float KrigingInterpolate(Vector2f queryPoint)
    {
        int n = SIZE;

        // Initialize matrices using Unity's types
        float[,] covarianceMatrix = new float[n, n];
        float[] covarianceVector = new float[n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                float distance = Vector2f.Distance(dataPoints[i], dataPoints[j]);
                float covValue = sill - sill * Mathf.Pow(1.5f * (distance / range), 1.5f) +
                                 0.5f * Mathf.Pow(1.5f * (distance / range), 2.5f);

                covarianceMatrix[i, j] = covValue;
            }

            float queryDistance = Vector2f.Distance(queryPoint, dataPoints[i]);
            float queryCovValue = sill - sill * Mathf.Pow(1.5f * (queryDistance / range), 1.5f) +
                                  0.5f * Mathf.Pow(1.5f * (queryDistance / range), 2.5f);
            covarianceVector[i] = queryCovValue;
        }

        // Solve the system of linear equations
        float[] weights = SolveLinearSystem(covarianceMatrix, covarianceVector);

        // Calculate the interpolated value
        float interpolatedValue = 0.0f;
        for (int i = 0; i < n; i++)
        {
            float distance = Vector2f.Distance(queryPoint, dataPoints[i]);
            float covValue = sill - sill * Mathf.Pow(1.5f * (distance / range), 1.5f) +
                             0.5f * Mathf.Pow(1.5f * (distance / range), 2.5f);

            interpolatedValue += weights[i] * (values[i] - nugget);
        }

        // Add the nugget effect
        interpolatedValue += nugget;

        return interpolatedValue;
    }

    float[] SolveLinearSystem(float[,] matrix, float[] vector)
    {
        int n = matrix.GetLength(0);
        float[] result = new float[n];

        for (int k = 0; k < n - 1; k++)
        {
            for (int i = k + 1; i < n; i++)
            {
                float factor = matrix[i, k] / matrix[k, k];

                for (int j = k; j < n; j++)
                {
                    matrix[i, j] -= factor * matrix[k, j];
                }

                vector[i] -= factor * vector[k];
            }
        }

        for (int i = n - 1; i >= 0; i--)
        {
            result[i] = vector[i];

            for (int j = i + 1; j < n; j++)
            {
                result[i] -= matrix[i, j] * result[j];
            }

            result[i] /= matrix[i, i];
        }

        return result;
    }
}
public class InsidePolygonHelper
{
  public class line {
    public Vector2f p1, p2;
    public line(Vector2f p1, Vector2f p2)
    {
      this.p1=p1;
      this.p2=p2;
    }
  }
 
  static int onLine(line l1, Vector2f p)
  {
    // Check whether p is on the line or not
    if (p.x <= Mathf.Max(l1.p1.x, l1.p2.x)
        && p.x >= Mathf.Min(l1.p1.x, l1.p2.x)
        && p.y <= Mathf.Max(l1.p1.y, l1.p2.y)
            && p.y >= Mathf.Min(l1.p1.y, l1.p2.y))
      return 1;
 
    return 0;
  }
 
  static float direction(Vector2f a, Vector2f b, Vector2f c)
  {
    float val = (b.y - a.y) * (c.x - b.x)
      - (b.x - a.x) * (c.y - b.y);
 
    if (val == 0)
 
      // Collinear
      return 0;
 
    else if (val < 0)
 
      // Anti-clockwise direction
      return 2;
 
    // Clockwise direction
    return 1;
  }
 
  static int isIntersect(line l1, line l2)
  { 
    // Four direction for two lines and points of other line
    float dir1 = direction(l1.p1, l1.p2, l2.p1);
    float dir2 = direction(l1.p1, l1.p2, l2.p2);
    float dir3 = direction(l2.p1, l2.p2, l1.p1);
    float dir4 = direction(l2.p1, l2.p2, l1.p2);
 
    // When intersecting
    if (dir1 != dir2 && dir3 != dir4)
      return 1;
 
    // When p2 of line2 are on the line1
    if (dir1 == 0 && onLine(l1, l2.p1)==1)
      return 1;
 
    // When p1 of line2 are on the line1
    if (dir2 == 0 && onLine(l1, l2.p2)==1)
      return 1;
 
    // When p2 of line1 are on the line2
    if (dir3 == 0 && onLine(l2, l1.p1)==1)
      return 1;
 
    // When p1 of line1 are on the line2
    if (dir4 == 0 && onLine(l2, l1.p2)==1)
      return 1;
 
    return 0;
  }
 
  public static int checkInside(List<Vector2f> poly, int n, Vector2f p)
  {
 
    // When polygon has less than 3 edge, it is not polygon
    if (n < 3)
      return 0;
 
    // Create a point at infinity, y is same as point p
    Vector2f pt=new Vector2f(9999, p.y);
    line exline = new line(p,pt); 
    int count = 0;
    int i = 0;
    do {
 
      // Forming a line from two consecutive points of
      // poly
      line side = new line( poly[i], poly[(i + 1) % n] );
      if (isIntersect(side, exline)==1) {
 
        // If side is intersects exline
        if (direction(side.p1, p, side.p2) == 0)
          return onLine(side, p);
        count++;
      }
      i = (i + 1) % n;
    } while (i != 0);
 
    // When count is odd
    return count & 1;
  }
 
  // Driver code
  public static void Main(string[] args)
  {
    List<Vector2f> polygon
      = new List<Vector2f>{ new Vector2f(0, 0), new Vector2f(10, 0), new Vector2f(10, 10), new Vector2f(0, 10) };
    Vector2f p = new Vector2f(5, 3 );
    //int n = 4;
 
    /* // Function call
    if (checkInside(polygon, n, p)==1)
      Console.WriteLine("Point is inside.");
    else
      Console.WriteLine("Point is outside."); */
  }

}
