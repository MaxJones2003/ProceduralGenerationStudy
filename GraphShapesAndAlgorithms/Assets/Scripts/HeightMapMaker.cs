using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Map;
using UnityEngine;
using Debug = UnityEngine.Debug;
public class HeightMapMaker
{
    public static (float, EBiomeType)[,] Go(float scale, int height, int width, List<Center> centers, out float maxHeight, out float minHeight)
    {
        Debug.Log("Starting HeightMapMaker");
        #region Variables
        KDTree<Center> tree = new KDTree<Center>();
        maxHeight = float.MinValue;
        minHeight = float.MaxValue;
        float maxH = float.MinValue;
        float minH = float.MaxValue;
        (float, EBiomeType)[,] heightMap = new (float, EBiomeType)[height, width];
        #endregion

        var stages = new List<Tuple<string, Action>>();

        void TimeIt(string name, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            Debug.Log($"{name}: {stopwatch.ElapsedMilliseconds} ms");
        }

        stages.Add(new Tuple<string, Action>("Build Tree...",
            () =>
            {
                tree.Insert(centers);
            }));
        stages.Add(new Tuple<string, Action>("Find Nearest Center...",
            () =>
            {
                // Iterate over the range of x and y values
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int trev = 0;
                        var pos = new Vector2f(x, y);
                        // Find the nearest point in the quadtree
                        var nearestCenters = tree.FindNearestNeighbors(pos, 5);
                        
                        Center nearestCenter = nearestCenters.Where(c => c.Inside(pos)).FirstOrDefault();
                        if(nearestCenter == null) nearestCenter = nearestCenters[0];

                        heightMap[y, x].Item2 = nearestCenter.biomeEnum;
                        //var nearestCenter = tree.FindNearestNeighbor(pos);
                        // Assign the elevation of the nearest point to the height of the point (x, y)
                        heightMap[y, x].Item1 = nearestCenter.elevation * scale;
                        maxH = heightMap[y, x].Item1 > maxH ? heightMap[y, x].Item1 : maxH;
                        minH = heightMap[y, x].Item1 < minH ? heightMap[y, x].Item1 : minH;
                    }
                }
            }));
        /* stages.Add(new Tuple<string, Action>("Interpolate...",
            () =>
            {
                // Create a 2D array for the height map
                heightMap = new float[height, width];
                maxH = float.MinValue;
                Vector2f pos = new Vector2f();
                // Iterate over the range of x and y values
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Find the nearest point in the quadtree
                        pos = new Vector2f(x, y);
                        var nearestCorners = nearestCornersMap[y, x];
                        // Assign the elevation of the nearest point to the height of the point (x, y)
                        heightMap[y, x] = IDWInterpolator(nearestCorners, pos) * scale;
                        
                        maxH = heightMap[y, x] > maxH ? heightMap[y, x] : maxH;
                        minH = heightMap[y, x] < minH ? heightMap[y, x] : minH;
                    }
                }
            })); */
        stages.Add(new Tuple<string, Action>("Normalize...",
            () =>
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        heightMap[y, x].Item1 = (heightMap[y, x].Item1 - minH) / (maxH - minH);
                    }
                }
            }));

        for (int i = 0; i < stages.Count; i++)
        {
            TimeIt(stages[i].Item1, stages[i].Item2);
        }

        maxHeight = maxH;
        minHeight = minH;

        return heightMap;
    }
    
    public static float[,] Go(float scale, int height, int width, List<Corner> corners, out float maxHeight, out float minHeight)
    {
        Debug.Log("Starting HeightMapMaker");
        #region Variables
        NewKDTree.KDTree tree = new NewKDTree.KDTree(corners);
        maxHeight = float.MinValue;
        minHeight = float.MaxValue;
        float maxH = float.MinValue;
        float minH = float.MaxValue;
        float[,] heightMap = new float[height, width];
        #endregion

        var stages = new List<Tuple<string, Action>>();

        void TimeIt(string name, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            Debug.Log($"{name}: {stopwatch.ElapsedMilliseconds} ms");
        }

        stages.Add(new Tuple<string, Action>("Build Tree...",
            () =>
            {
                //tree.Insert(corners);
            }));

        Corner[,][] nearestCornersMap = new Corner[height,width][];
        List<Corner> nearestCorners = new List<Corner>();
        stages.Add(new Tuple<string, Action>("Find Nearest Point List...",
            () =>
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        nearestCorners = tree.NearestNeighbors(new Vector2f(x, y), 5);
                        nearestCornersMap[y, x] = nearestCorners.ToArray();
                    }
                }
            }));
        stages.Add(new Tuple<string, Action>("Interpolate...",
            () =>
            {
                // Create a 2D array for the height map
                heightMap = new float[height, width];
                maxH = float.MinValue;
                // Iterate over the range of x and y values
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Find the nearest point in the quadtree
                        var nearestCorners = nearestCornersMap[y, x];
                        // Assign the elevation of the nearest point to the height of the point (x, y)
                        heightMap[y, x] = IDWInterpolator(nearestCorners, new Vector2f(x, y)) * scale;
                        
                        maxH = heightMap[y, x] > maxH ? heightMap[y, x] : maxH;
                        minH = heightMap[y, x] < minH ? heightMap[y, x] : minH;
                    }
                }
            }));
        stages.Add(new Tuple<string, Action>("Normalize...",
            () =>
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        heightMap[y, x] = (heightMap[y, x] - minH) / (maxH - minH);
                    }
                }
            }));

        for (int i = 0; i < stages.Count; i++)
        {
            TimeIt(stages[i].Item1, stages[i].Item2);
        }

        heightMap = SmoothArray(heightMap, 10, out maxH, out minH);

        maxHeight = maxH;
        minHeight = minH;

        return heightMap;
    }
    public static float[,] GenerateHeightMapNormalized(float scale, int height, int width, List<Corner> corners, out float maxHeight, out float minHeight)
    {
        maxHeight = float.MinValue;
        minHeight = float.MaxValue;

        var tree = new KDTree<Corner>();

        tree.Insert(corners);

        // Create a 2D array for the height map
        float[,] heightMap = new float[height, width];

        // Iterate over the range of x and y values
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Find the nearest point in the quadtree
                var nearestCorner = tree.FindNearestNeighbor(new Vector2f(x, y));

                // Assign the elevation of the nearest point to the height of the point (x, y)
                heightMap[y, x] = nearestCorner.elevation * scale;
                maxHeight = heightMap[y, x] > maxHeight ? heightMap[y, x] : maxHeight;
                minHeight = heightMap[y, x] < minHeight ? heightMap[y, x] : minHeight;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heightMap[y, x] = (heightMap[y, x] - minHeight) / (maxHeight - minHeight);
            }
        }

        return heightMap;
    }
    public static float[,] GenerateHeightMapNormalizedCenters(float scale, int height, int width, List<Center> centers, out float maxHeight, out float minHeight)
    {
        maxHeight = float.MinValue;
        minHeight = float.MaxValue;

        var tree = new KDTree<Center>();

        tree.Insert(centers);

        // Create a 2D array for the height map
        float[,] heightMap = new float[height, width];

        // Iterate over the range of x and y values
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Find the nearest point in the quadtree
                var nearestCenter = tree.FindNearestNeighbor(new Vector2f(x, y));

                // Assign the elevation of the nearest point to the height of the point (x, y)
                heightMap[y, x] = nearestCenter.elevation * scale;
                maxHeight = heightMap[y, x] > maxHeight ? heightMap[y, x] : maxHeight;
                minHeight = heightMap[y, x] < minHeight ? heightMap[y, x] : minHeight;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heightMap[y, x] = (heightMap[y, x] - minHeight) / (maxHeight - minHeight);
            }
        }

        return heightMap;
    }
    public static float[,] GenerateHeightMapInterpolatedNormalized(float scale, int height, int width, List<Corner> corners, out float maxHeight, out float minHeight)
    {
        maxHeight = float.MinValue;
        minHeight = float.MaxValue;

        var tree = new KDTree<Corner>();

        tree.Insert(corners);

        // Create a 2D array for the height map
        float[,] heightMap = new float[height, width];

        // Iterate over the range of x and y values
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Find the nearest point in the quadtree
                Vector2f pos = new Vector2f(x, y);
                var nearestCorners = tree.FindNearestNeighbors(pos, 5);

                // Assign the elevation of the nearest point to the height of the point (x, y)
                heightMap[y, x] = IDWInterpolator(nearestCorners, pos) * scale;
                
                maxHeight = heightMap[y, x] > maxHeight ? heightMap[y, x] : maxHeight;
                minHeight = heightMap[y, x] < minHeight ? heightMap[y, x] : minHeight;
            }
        }

        // Normalize the height map
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (heightMap[y, x] < minHeight)
                {
                    minHeight = heightMap[y, x];
                }
                else if (heightMap[y, x] > maxHeight)
                {
                    maxHeight = heightMap[y, x];
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heightMap[y, x] = (heightMap[y, x] - minHeight) / (maxHeight - minHeight);
            }
        }

        return heightMap;
    }
    public static float[,] GenerateHeightMapInterpolatedNormalizedNew(float scale, int height, int width, List<Corner> corners, out float maxHeight, out float minHeight)
    {
        maxHeight = float.MinValue;
        minHeight = float.MaxValue;

        Vector3[] positions = new Vector3[corners.Count];

        for (int i = 0; i < corners.Count; i++)
        {
            positions[i] = new Vector3(corners[i].point.x, corners[i].point.y, corners[i].elevation);
        }
        // Create a 2D array for the height map
        float[,] heightMap = new float[height, width];

        // Iterate over the range of x and y values
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Find the nearest point in the quadtree
                Vector2f pos = new Vector2f(x, y);


                // Assign the elevation of the nearest point to the height of the point (x, y)
                heightMap[y, x] = IDWInterpolator(positions, pos) * scale;
                
                maxHeight = heightMap[y, x] > maxHeight ? heightMap[y, x] : maxHeight;
                minHeight = heightMap[y, x] < minHeight ? heightMap[y, x] : minHeight;
            }
        }

        // Normalize the height map
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (heightMap[y, x] < minHeight)
                {
                    minHeight = heightMap[y, x];
                }
                else if (heightMap[y, x] > maxHeight)
                {
                    maxHeight = heightMap[y, x];
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heightMap[y, x] = (heightMap[y, x] - minHeight) / (maxHeight - minHeight);
            }
        }

        return heightMap;
    }

    struct corner{
        public Vector2f position;
        public float height;
    }
    public static float[,] ComputeShaderHeightMap(ComputeShader heightMapComputeShader, ComputeShader gaussianComputeShader, float scale, int width, List<Corner> Corners)
    {
        int heightSize = sizeof(float);
        int positionSize = heightSize * 2;

        RenderTexture heightMapTexture = new RenderTexture(width, width, 0, RenderTextureFormat.RFloat);
        heightMapTexture.enableRandomWrite = true;
        heightMapTexture.Create();

        int kernelHandle = heightMapComputeShader.FindKernel("CSMain");
        heightMapComputeShader.SetTexture(kernelHandle, "heightMap", heightMapTexture);


        corner[] corners = new corner[Corners.Count];
        for(int i = 0; i < Corners.Count; i++)
        {
            corners[i] = new corner { position = Corners[i].point, height = Corners[i].elevation };
        }

        ComputeBuffer cornersBuffer = new ComputeBuffer(corners.Length, positionSize + heightSize);
        cornersBuffer.SetData(corners);

        float[] data = new float[width * width];
        ComputeBuffer heightMapBuffer = new ComputeBuffer(width * width, sizeof(float));

        
        heightMapComputeShader.SetBuffer(0, "corners", cornersBuffer);
        heightMapComputeShader.SetBuffer(0, "heightMap", heightMapBuffer);
        heightMapComputeShader.SetInt("cornersLen", corners.Length);
        heightMapComputeShader.SetInt("width", width);
        heightMapBuffer.SetData(data); 

        gaussianComputeShader.SetBuffer(0, "heightMap", heightMapBuffer);
        gaussianComputeShader.SetInt("width", width);
        gaussianComputeShader.SetInt("maxIndex", width * width);

        heightMapComputeShader.Dispatch(0, width / 8, width / 8, 1);
        gaussianComputeShader.Dispatch(0, width / 8, width / 8, 1);

        heightMapBuffer.GetData(data);

        // Convert the flat array into a 2D array
        float[,] heightMap2D = new float[width, width];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                heightMap2D[x, y] = data[y * width + x];
            }
        }
    
        // get the min and max values for normalizing the values between 0-1
        float highest = float.MinValue;
        float lowest = float.MaxValue;

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float value = heightMap2D[x, y];
                highest = Mathf.Max(highest, value);
                lowest = Mathf.Min(lowest, value);
            }
        }
        // normalize the values
        float range = highest - lowest;
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < width; x++)
            {
                heightMap2D[x, y] = (heightMap2D[x, y] - lowest) / range;
            }
        } 



        cornersBuffer.Release();
        heightMapBuffer.Release();

        return heightMap2D;
    }
    public static float IDWInterpolator(Corner[] corners, Vector2f queryPoint, float power = 2)
    {
        float totalWeight = 0;
        float weightedSum = 0;

        foreach (var corner in corners)
        {
            float distance = Vector2f.Distance(queryPoint, corner.Position);

            // If the query point is exactly at a corner, return the elevation of that corner
            if (distance == 0)
            {
                return corner.elevation;
            }

            float weight = 1 / Mathf.Pow(distance, power);
            totalWeight += weight;
            weightedSum += weight * corner.elevation;
        }
        return weightedSum / totalWeight;
    }
    /// <summary>
    /// The X and Y of the vector3 are the position, the z is the elevation
    /// </summary>
    /// <param name="positions"></param>
    /// <param name="queryPoint"></param>
    /// <param name="power"></param>
    /// <returns></returns>
    public static float IDWInterpolator(Vector3[] positions, Vector2f queryPoint, float power = 2)
    {
        float totalWeight = 0;
        float weightedSum = 0;

        foreach (var pos3 in positions)
        {
            Vector2f pos2 = new Vector2f(pos3.x, pos3.y);
            float distance = Vector2f.Distance(queryPoint, pos2);

            // If the query point is exactly at a corner, return the elevation of that corner
            if (distance == 0)
            {
                return pos3.z;
            }

            float weight = 1 / Mathf.Pow(distance, power);
            totalWeight += weight;
            weightedSum += weight * pos3.z;
        }
        return weightedSum / totalWeight;
    }

    public static float[,] SmoothArray(float[,] input, int filterSize, out float minValue, out float maxValue)
    {
        int height = input.GetLength(0);
        int width = input.GetLength(1);
        float[,] output = new float[height, width];

        minValue = float.MaxValue;
        maxValue = float.MinValue;

        for (int y =  0; y < height; y++)
        {
            for (int x =  0; x < width; x++)
            {
                float sum =  0;
                int count =  0;

                for (int dy = -filterSize /  2; dy <= filterSize /  2; dy++)
                {
                    for (int dx = -filterSize /  2; dx <= filterSize /  2; dx++)
                    {
                        int newY = y + dy;
                        int newX = x + dx;

                        // Check if the new coordinates are within the array bounds
                        if (newY >=  0 && newY < height && newX >=  0 && newX < width)
                        {
                            sum += input[newY, newX];
                            count++;
                        }
                    }
                }

                // Calculate the average and assign it to the output array
                float average = sum / count;
                output[y, x] = average;

                // Update min and max values
                minValue = Math.Min(minValue, average);
                maxValue = Math.Max(maxValue, average);
            }
        }

        return output;
    }


    /* public float[,] GenerateHeightMap(float scale, int height, int width, List<Corner> corners)
    {
        var tree = new KdTree<float, Corner>(2, new FloatMath());
        
        foreach (Corner corner in corners)
        {
            tree.Add(new [] {corner.point.x, corner.point.y}, corner);
        }

        // Create a 2D array for the height map
        float[,] heightMap = new float[height, width];

        // Iterate over the range of x and y values
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Find the nearest point in the quadtree
                var nearestCorner = tree.GetNearestNeighbours(new[] { (float)x, (float)y }, 1)[0].Value;

                // Assign the elevation of the nearest point to the height of the point (x, y)
                heightMap[y, x] = nearestCorner.elevation * scale;
            }
        }

        return heightMap;
    } */
}