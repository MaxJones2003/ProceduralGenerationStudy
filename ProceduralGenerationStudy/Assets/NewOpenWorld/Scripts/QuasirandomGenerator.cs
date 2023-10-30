using UnityEngine;
using System;

public static class QuasirandomGenerator
{
    private static System.Random random = new System.Random();


    // Specify the base for each dimension in the Halton sequence.
    public static int baseX = 2;  // Base for the first dimension
    public static int baseY = 3;  // Base for the second dimension

    public static float GenerateRandomValue(float minValue, float maxValue)
    {
        float randInitial = random.Next((int)minValue+2, (int)maxValue-1);
       return randInitial + random.Next(-1, 1)/0.3f;
    }


    // Halton sequence generator for 2D points.

}
