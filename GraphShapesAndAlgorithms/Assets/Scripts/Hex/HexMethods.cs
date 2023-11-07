using System;
using System.Collections.Generic;

public static class HexMethods
{
    public static int Hex_Length(Hex hex)
    {
        return (Math.Abs(hex.q) + Math.Abs(hex.r) + Math.Abs(hex.s)) / 2;
    }
    public static int Hex_Distance(Hex a, Hex b)
    {
        return Hex_Length(a - b);
    }
    private static readonly Hex[] hexDirections = {
        new Hex(1, 0, -1), new Hex(1, -1, 0), new Hex(0, -1, 1),
        new Hex(-1, 0, 1), new Hex(-1, 1, 0), new Hex(0, 1, -1)
    };

    public static Hex Hex_Direction(int direction)
    {
        if (direction < 0 || direction >= 6)
            throw new ArgumentException("Direction must be between 0 and 5");

        return hexDirections[direction];
    }

    public static Hex Hex_Neighbor(Hex hex, int direction)
    {
        return hex + Hex_Direction(direction);
    }
    public const int EVEN = 1;
    public const int ODD = -1;

    public static OffsetCoord Q_Offset_From_Cube(int offset, Hex h)
    {
        int col = h.q;
        int row = h.r + (h.q + offset * (h.q & 1)) / 2;
        if (offset != EVEN && offset != ODD)
        {
            throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
        }
        return new OffsetCoord(col, row);
    }

    public static Hex Q_Offset_To_Cube(int offset, OffsetCoord h)
    {
        int q = h.col;
        int r = h.row - (h.col + offset * (h.col & 1)) / 2;
        int s = -q - r;
        if (offset != EVEN && offset != ODD)
        {
            throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
        }
        return new Hex(q, r, s);
    }

    public static OffsetCoord R_Offset_From_Cube(int offset, Hex h)
    {
        int col = h.q + (h.r + offset * (h.r & 1)) / 2;
        int row = h.r;
        if (offset != EVEN && offset != ODD)
        {
            throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
        }
        return new OffsetCoord(col, row);
    }

    public static Hex R_Offset_To_Cube(int offset, OffsetCoord h)
    {
        int q = h.col - (h.row + offset * (h.row & 1)) / 2;
        int r = h.row;
        int s = -q - r;
        if (offset != EVEN && offset != ODD)
        {
            throw new ArgumentException("offset must be EVEN (+1) or ODD (-1)");
        }
        return new Hex(q, r, s);
    }
    public static Point Hex_To_Pixel(Layout layout, Hex h)
    {
        var M = layout.orientation;
        double x = (M.f0 * h.q + M.f1 * h.r) * layout.size.x;
        double y = (M.f2 * h.q + M.f3 * h.r) * layout.size.y;
        return new Point(x + layout.origin.x, y + layout.origin.y);
    }
    public static FractionalHex Pixel_To_Hex(Layout layout, Point p)
    {
        var M = layout.orientation;
        Point pt = new Point((p.x - layout.origin.x) / layout.size.x, 
                            (p.y - layout.origin.y) / layout.size.y);
        double q = M.b0 * pt.x + M.b1 * pt.y;
        double r = M.b2 * pt.x + M.b3 * pt.y;
        return new FractionalHex(q, r, -q - r);
    }
    public static Point Hex_Corner_Offset(Layout layout, int corner)
    {
        var size = layout.size;
        double angle = 2.0 * Math.PI * (layout.orientation.startAngle + corner) / 6;
        return new Point(size.x * Math.Cos(angle), size.y * Math.Sin(angle));
    }
    public static Point[] Polygon_Corners(Layout layout, Hex h)
    {
        var corners = new Point[6];
        var center = Hex_To_Pixel(layout, h);
        for (int i = 0; i < 6; i++)
        {
            var offset = Hex_Corner_Offset(layout, i);
            corners[i] = new Point(center.x + offset.x, center.y + offset.y);
        }
        return corners;
    }
    public static Hex Hex_Round(FractionalHex h)
    {
        int q = (int)Math.Round(h.q);
        int r = (int)Math.Round(h.r);
        int s = (int)Math.Round(h.s);
        double q_diff = Math.Abs(q - h.q);
        double r_diff = Math.Abs(r - h.r);
        double s_diff = Math.Abs(s - h.s);
        if (q_diff > r_diff && q_diff > s_diff)
        {
            q = -r - s;
        }
        else if (r_diff > s_diff)
        {
            r = -q - s;
        }
        else
        {
            s = -q - r;
        }
        return new Hex(q, r, s);
    }
    public static double Lerp(double a, double b, double t)
    {
        return a * (1 - t) + b * t;
    }

    public static FractionalHex Hex_Lerp(Hex a, Hex b, double t)
    {
        return new FractionalHex(Lerp(a.q, b.q, t), Lerp(a.r, b.r, t), Lerp(a.s, b.s, t));
    }

    public static FractionalHex Hex_Lerp(FractionalHex a, FractionalHex b, double t)
    {
        return new FractionalHex(Lerp(a.q, b.q, t), Lerp(a.r, b.r, t), Lerp(a.s, b.s, t));
    }

    public static List<Hex> HexLineDraw(Hex a, Hex b)
    {
        int N = Hex_Distance(a, b);
        FractionalHex a_nudge = new FractionalHex(a.q + 1e-6, a.r + 1e-6, a.s - 2e-6);
        FractionalHex b_nudge = new FractionalHex(b.q + 1e-6, b.r + 1e-6, b.s - 2e-6);
        List<Hex> results = new List<Hex>();
        double step = 1.0 / Math.Max(N, 1);
        for (int i = 0; i <= N; i++)
        {
            results.Add(Hex_Round(Hex_Lerp(a_nudge, b_nudge, step * i)));
        }
        return results;
    }
}
