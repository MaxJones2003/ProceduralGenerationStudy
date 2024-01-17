namespace QT
{
    public struct Point<T> where T : notnull
    {
        public T Value { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public Point(T value, float x, float y)
        {
            Value = value;
            X = x;
            Y = y;
        }

        public static Point<T> Middle(Point<T> p1, Point<T> p2)
        {
            return new Point<T>(p1.Value, (p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }
    }
}
