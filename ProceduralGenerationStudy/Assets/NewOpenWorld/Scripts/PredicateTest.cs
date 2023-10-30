using TriangleNet.Geometry;

using TriangleNet;

public class CustomPredicates : IPredicates
{
    public double CounterClockwise(Point a, Point b, Point c)
    {
        // Implement the CounterClockwise predicate here.
        // Return a positive value if (a, b, c) is in counter-clockwise order.
        // Return a negative value if it's in clockwise order.
        // Return zero if the points are collinear.

        // Example:
        double det = (a.X * b.Y + b.X * c.Y + c.X * a.Y) -
                        (b.X * a.Y + c.X * b.Y + a.X * c.Y);
        return det;
    }

    public double InCircle(Point a, Point b, Point c, Point p)
    {
        // Implement the InCircle predicate here.
        // Return a positive value if point p lies inside the circle passing through a, b, and c.
        // Return a negative value if it lies outside.
        // Return zero if the points are cocircular.

        // Example:
        double ab = (a.X * a.X + a.Y * a.Y);
        double bc = (b.X * b.X + b.Y * b.Y);
        double cd = (c.X * c.X + c.Y * c.Y);
        double pd = (p.X * p.X + p.Y * p.Y);

        double det = (ab * (b.Y * pd - p.Y * b.X) - bc * (a.Y * pd - p.Y * a.X) + cd * (a.Y * b.X - b.Y * a.X));
        return det;
    }

    public Point FindCircumcenter(Point org, Point dest, Point apex, ref double xi, ref double eta)
    {
        // Implement the FindCircumcenter method here.
        // Calculate the circumcenter and return it.
        
        // Example:
        double D = 2 * (org.X * (dest.Y - apex.Y) + dest.X * (apex.Y - org.Y) + apex.X * (org.Y - dest.Y));
        xi = ((org.X * org.X + org.Y * org.Y) * (dest.Y - apex.Y) + (dest.X * dest.X + dest.Y * dest.Y) * (apex.Y - org.Y) + (apex.X * apex.X + apex.Y * apex.Y) * (org.Y - dest.Y)) / D;
        eta = ((org.X * org.X + org.Y * org.Y) * (apex.X - dest.X) + (dest.X * dest.X + dest.Y * dest.Y) * (org.X - apex.X) + (apex.X * apex.X + apex.Y * apex.Y) * (dest.X - org.X)) / D;

        return new Point(xi, eta);
    }

    public Point FindCircumcenter(Point org, Point dest, Point apex, ref double xi, ref double eta, double offconstant)
    {
        // Implement the FindCircumcenter method with an off-center constant here.
        // Calculate the circumcenter (or off-center) and return it.
        
        // Example:
        double D = 2 * (org.X * (dest.Y - apex.Y) + dest.X * (apex.Y - org.Y) + apex.X * (org.Y - dest.Y));
        xi = ((org.X * org.X + org.Y * org.Y) * (dest.Y - apex.Y) + (dest.X * dest.X + dest.Y * dest.Y) * (apex.Y - org.Y) + (apex.X * apex.X + apex.Y * apex.Y) * (org.Y - dest.Y)) / D;
        eta = ((org.X * org.X + org.Y * org.Y) * (apex.X - dest.X) + (dest.X * dest.X + dest.Y * dest.Y) * (org.X - apex.X) + (apex.X * apex.X + apex.Y * apex.Y) * (dest.X - org.X)) / D;

        // Adjust the circumcenter with the off-center constant
        xi += offconstant;
        eta += offconstant;

        return new Point(xi, eta);
    }
}
