public static class PRandom
{
    public static float NextDouble(System.Random random)
    {
        return (float)random.NextDouble();
    }

    public static int NextIntRange(System.Random random, int min, int max)
    {
        return random.Next(min, max);
    }

    public static float NextDoubleRange(System.Random random, float min, float max)
    {
        return min + (NextDouble(random) * (max - min));
    }
}
