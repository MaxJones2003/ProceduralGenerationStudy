using System;

[System.Serializable]
public struct Orientation
{
    public readonly double f0, f1, f2, f3;
    public readonly double b0, b1, b2, b3;
    public readonly double startAngle; // In multiples of 60°
    public Orientation(double f0, double f1, double f2, double f3,
                double b0, double b1, double b2, double b3,
                double startAngle)
    {
        this.f0 = f0; this.f1 = f1; this.f2 = f2; this.f3 = f3;
        this.b0 = b0; this.b1 = b1; this.b2 = b2; this.b3 = b3;
        this.startAngle = startAngle;
    }

    public static Orientation layout_flat
    {
        get
        {
            return new Orientation(
                3.0f / 2.0f, 0.0f, Math.Sqrt(3.0f) / 2.0f, Math.Sqrt(3.0f),
                2.0f / 3.0f, 0.0f, -1.0f / 3.0f, Math.Sqrt(3.0f) / 3.0f,
                0.0f
            );
        }
    }
    public static Orientation layout_pointy
    {
        get
        {
            return new Orientation(
                Math.Sqrt(3.0f), Math.Sqrt(3.0f) / 2.0f, 0.0f, 3.0f / 2.0f,
                Math.Sqrt(3.0f) / 3.0f, -1.0f / 3.0f, 0.0f, 2.0f / 3.0f,
                0.5f
            );
        }
    }
}
