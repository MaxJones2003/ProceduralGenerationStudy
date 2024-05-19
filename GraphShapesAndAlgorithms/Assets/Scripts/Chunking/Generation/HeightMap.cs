
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Generation
{
    public class HeightMap
    {
        struct Point{
            public Vector2f position;
            public float height;
        }
        // EVENTUALLY NEED TO ADD AN OFFSET VALUE FOR WHEN I ONLY GENERATE A SECTION OF THE HEIGHT MAP AT A TIME
        public static float[] GenerateHeights(int size, List<Map.Corner> corners)
        {
            
            // Get the byte size of the values being passed
            int heightSize = sizeof(float);
            int positionSize = heightSize * 2;

            // Load the compute shaders
            ComputeShader heightMapCompute = StaticResourcesLoader.HeightMapCompute;
            ComputeShader gaussianCompute = StaticResourcesLoader.GaussianBlurCompute;

            // Get the kernels from the compute shaders
            int heightKernel = heightMapCompute.FindKernel("HeightMap");
            int gaussianKernel = gaussianCompute.FindKernel("GaussianBlur");

            int width = GridMetrics.Width * size;
            // Convert the corner class to a struct the gpu can use for interpolation
            var validCorners = corners.Where(c => c.point.x >= 0 && c.point.x <= width && c.point.y >= 0 && c.point.y <= width);
            Point[] points = validCorners.Select(c => new Point { position = c.point, height = c.elevation }).ToArray();

            // Create the compute buffer that contains the list of corners in struct form
            ComputeBuffer cornersBuffer = new ComputeBuffer(points.Length, positionSize + heightSize);
            cornersBuffer.SetData(points);

            // Create an array that is width * width long
            float[] heightData = new float[width * width];

            // Create the buffer to contain the height map array
            ComputeBuffer heightMapBuffer = new ComputeBuffer(width * width, heightSize);
            heightMapBuffer.SetData(heightData); 
            
            // Initialize values of the height map making compute shader, set corners buffer for reading and the heightmap for writing
            // Set the cornersLen to the length of the array, set the width to GridMetrics.PointsPerChunk
            heightMapCompute.SetBuffer(heightKernel, "corners", cornersBuffer);
            heightMapCompute.SetBuffer(heightKernel, "heightMap", heightMapBuffer);
            heightMapCompute.SetInt("cornersLen", points.Length);
            heightMapCompute.SetInt("width", width);

            // Initialize the values for the Gaussian Blur compute that will reduce spikes caused by cheap interpolation
            gaussianCompute.SetBuffer(gaussianKernel, "heightMap", heightMapBuffer);
            gaussianCompute.SetInt("width", width);
            gaussianCompute.SetInt("maxIndex", width * width);
            
            // Call functions in the compute shaders
            int threadGroups = width / GridMetrics.NumThreads;
            heightMapCompute.Dispatch(heightKernel, threadGroups, threadGroups, 1);
            gaussianCompute.Dispatch(gaussianKernel, threadGroups, threadGroups, 1);

            // Get the data from the shaders
            heightMapBuffer.GetData(heightData);

            cornersBuffer.Release();
            heightMapBuffer.Release();

            /* float highestValue = heightData.Max();
            Debug.Log("Highest Value: " + highestValue);

            // Find the lowest value in the array
            float lowestValue = heightData.Min();
            Debug.Log("Lowest Value: " + lowestValue); */

            return heightData;
            
        }

        public static float[] GenerateWeights(float[] heights, Vector2 offset, int size)
        {
            // get the byte size of float
            int heightSize = sizeof(float);
            
            int chunkWidth = GridMetrics.PointsPerChunk;
            int totalWidth = GridMetrics.Width * size;

            // Set up the weights array to be PointsPerChunk^3 long
            float[] cubeData = new float[chunkWidth * chunkWidth * chunkWidth];

            // Intialize the in height map buffer
            ComputeBuffer heightMapBuffer = new ComputeBuffer(heights.Length, heightSize);
            heightMapBuffer.SetData(heights); 

            // Initialize the compute buffer to contain the array
            ComputeBuffer cubesBuffer = new ComputeBuffer(chunkWidth * chunkWidth * chunkWidth, heightSize);
            cubesBuffer.SetData(cubeData);

            // Load the height map to cube weights compute shader
            ComputeShader heightToCubesCompute = StaticResourcesLoader.HeightMapToCubesCompute;

            // Set the thread groups size
            int heightToCubesKernel = heightToCubesCompute.FindKernel("HeightToCube");
            int threadGroups = GridMetrics.PointsPerChunk / GridMetrics.NumThreads;

            // Set the compute shader buffers, the in heights and the out weights, and the offset position
            heightToCubesCompute.SetBuffer(heightToCubesKernel, "_InHeights", heightMapBuffer);
            heightToCubesCompute.SetBuffer(heightToCubesKernel, "_OutWeights", cubesBuffer);
            heightToCubesCompute.SetVector("_Offset", offset);
            heightToCubesCompute.SetInt("_Width", totalWidth);
            heightToCubesCompute.SetInt("_ChunkSize", chunkWidth);

            // Call compute shader function
            heightToCubesCompute.Dispatch(heightToCubesKernel, threadGroups, threadGroups, threadGroups);

            cubesBuffer.GetData(cubeData);
            cubesBuffer.Release();

            return cubeData;
        }
        public static float[] GenerateWeights(float[] heights, Vector2 offset, int size, bool endX, bool endZ)
        {
            // get the byte size of float
            int heightSize = sizeof(float);
            
            int chunkHeight = GridMetrics.PointsPerChunk;
            int chunkWidthX = chunkHeight /* + (endX ? 0 : 1) */;
            int chunkWidthZ = chunkHeight /* + (endZ ? 0 : 1) */;
            int totalWidth = GridMetrics.Width * size;
            //int totalWidth = GridMetrics.Width * size;

            // Set up the weights array to be length * width * height long
            float[] cubeData = new float[chunkWidthX * chunkWidthZ * chunkHeight];

            // Intialize the in height map buffer
            ComputeBuffer heightMapBuffer = new ComputeBuffer(heights.Length, heightSize);
            heightMapBuffer.SetData(heights); 

            // Initialize the compute buffer to contain the array
            ComputeBuffer cubesBuffer = new ComputeBuffer(chunkWidthX * chunkWidthZ * chunkHeight, heightSize);
            cubesBuffer.SetData(cubeData);

            // Load the height map to cube weights compute shader
            ComputeShader heightToCubesCompute = StaticResourcesLoader.HeightMapToCubesCompute;

            // Set the thread groups size
            int heightToCubesKernel = heightToCubesCompute.FindKernel("HeightToCube");

            // Set the compute shader buffers, the in heights and the out weights, and the offset position
            heightToCubesCompute.SetBuffer(heightToCubesKernel, "_InHeights", heightMapBuffer);
            heightToCubesCompute.SetBuffer(heightToCubesKernel, "_OutWeights", cubesBuffer);
            heightToCubesCompute.SetVector("_Offset", offset);


            // IMPORTANT
            // I NEED TO ADD A LENGTH TO THIS COMPUTE SHADER SO THAT IT DOESN'T HAVE TO BE A SQUARE
            // THIS WILL ALLOW ME TO CONNECT DIFFERENT CHUNKS
            heightToCubesCompute.SetInt("_Width", totalWidth);
            heightToCubesCompute.SetInt("_numOfPointsXAxis", chunkWidthX);
            heightToCubesCompute.SetInt("_numOfPointsYAxis", chunkHeight);
            heightToCubesCompute.SetInt("_numOfPointsZAxis", chunkWidthZ);

            // Call compute shader function
            int threadGroups = GridMetrics.PointsPerChunk / GridMetrics.NumThreads;
            int threadGroupsX = chunkWidthX / GridMetrics.NumThreads;
            int threadGroupsZ = chunkWidthZ / GridMetrics.NumThreads;
            heightToCubesCompute.Dispatch(heightToCubesKernel, threadGroupsX, threadGroups, threadGroupsZ);

            cubesBuffer.GetData(cubeData);
            cubesBuffer.Release();

            return cubeData;
        }

        public static float[] GenerateWeights(ComputeBuffer pointsBuffer, float[] heights, Vector2 offset, int size, bool endX, bool endZ)
        {
            // get the byte size of float
            int heightSize = sizeof(float);
            
            int chunkHeight = GridMetrics.PointsPerChunk;
            int chunkWidthX = chunkHeight /* + (endX ? 0 : 1) */;
            int chunkWidthZ = chunkHeight /* + (endZ ? 0 : 1) */;
            int totalWidth = GridMetrics.Width * size;
            //int totalWidth = GridMetrics.Width * size;

            // Set up the weights array to be length * width * height long
            float[] cubeData = new float[chunkWidthX * chunkWidthZ * chunkHeight];

            // Intialize the in height map buffer
            ComputeBuffer heightMapBuffer = new ComputeBuffer(heights.Length, heightSize);
            heightMapBuffer.SetData(heights); 

            // Initialize the compute buffer to contain the array
            ComputeBuffer cubesBuffer = new ComputeBuffer(chunkWidthX * chunkWidthZ * chunkHeight, heightSize);
            cubesBuffer.SetData(cubeData);

            Vector4[] points = new Vector4[chunkWidthX * chunkWidthZ * chunkHeight];
            pointsBuffer.SetData(points);

            // Load the height map to cube weights compute shader
            ComputeShader heightToCubesCompute = StaticResourcesLoader.HeightMapToCubesCompute;

            // Set the thread groups size
            int heightToCubesKernel = heightToCubesCompute.FindKernel("HeightToCube");

            // Set the compute shader buffers, the in heights and the out weights, and the offset position
            heightToCubesCompute.SetBuffer(heightToCubesKernel, "_InHeights", heightMapBuffer);
            heightToCubesCompute.SetBuffer(heightToCubesKernel, "_OutWeights", cubesBuffer);
            heightToCubesCompute.SetBuffer(heightToCubesKernel, "points", pointsBuffer);
            heightToCubesCompute.SetVector("_Offset", offset);


            // IMPORTANT
            // I NEED TO ADD A LENGTH TO THIS COMPUTE SHADER SO THAT IT DOESN'T HAVE TO BE A SQUARE
            // THIS WILL ALLOW ME TO CONNECT DIFFERENT CHUNKS
            heightToCubesCompute.SetInt("_Width", totalWidth);
            heightToCubesCompute.SetInt("_numOfPointsXAxis", chunkWidthX);
            heightToCubesCompute.SetInt("_numOfPointsYAxis", chunkHeight);
            heightToCubesCompute.SetInt("_numOfPointsZAxis", chunkWidthZ);

            // Call compute shader function
            int threadGroups = GridMetrics.PointsPerChunk / GridMetrics.NumThreads;
            int threadGroupsX = chunkWidthX / GridMetrics.NumThreads;
            int threadGroupsZ = chunkWidthZ / GridMetrics.NumThreads;
            heightToCubesCompute.Dispatch(heightToCubesKernel, threadGroupsX, threadGroups, threadGroupsZ);

            cubesBuffer.GetData(cubeData);
            cubesBuffer.Release();
            heightMapBuffer.Release();

            return cubeData;
        }
    }
}
