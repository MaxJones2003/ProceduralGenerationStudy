using UnityEngine;

namespace Chunking
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public abstract class AbstractChunk : MonoBehaviour
    {
        private ComputeBuffer _trianglesBuffer;
        private ComputeBuffer _trianglesCountBuffer;
        private ComputeBuffer _weightsBuffer;
        private ComputeBuffer _points;
        protected float[] _weights;
        [HideInInspector]
        public bool endX;
        [HideInInspector]
        public bool endZ;
        struct Triangle
        {
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;

            public Vector3 this [int i] {
                get {
                    return i switch
                    {
                        0 => a,
                        1 => b,
                        _ => c,
                    };
                }
            }

            public static int SizeOf => sizeof(float) * 3 * 3;
        }
        public ESubChunkType ChunkType;
        public float[,] HeightMap;
        public int size = 1;

        public abstract void GenerateMesh();
        public abstract void GenerateMapData();


        /* void Awake()
        {
            CreateBuffers();
        }


        void OnDestroy()
        {
            ReleaseBuffers();
        } */
        public float iso = 1;

        protected virtual Mesh ConstructMesh()
        {
            ComputeShader MarchingShader = StaticResourcesLoader.MarchingCompute;
            MarchingShader.SetBuffer(0, "_Triangles", _trianglesBuffer);
            MarchingShader.SetBuffer(0, "_Weights", _weightsBuffer);
            MarchingShader.SetBuffer(0, "points", _points);

            int numPoints = GridMetrics.PointsPerChunk;
            int numPointsX = numPoints/*  + (endX ? 0 : 1) */;
            int numPointsZ = numPoints/*  + (endZ ? 0 : 1) */;

            MarchingShader.SetInt("_numOfPointsXAxis", numPointsX);
            MarchingShader.SetInt("_numOfPointsYAxis", numPoints);
            MarchingShader.SetInt("_numOfPointsZAxis", numPointsZ);
            MarchingShader.SetFloat("_IsoLevel", iso);

            _weightsBuffer.SetData(_weights);
            
            _trianglesBuffer.SetCounterValue(0);
            

            int threads = numPoints / GridMetrics.NumThreads;
            int threadsX = numPointsX / GridMetrics.NumThreads;
            int threadsZ = numPointsZ / GridMetrics.NumThreads;
            Debug.Log($"{threads} {threadsX} {threadsZ}");

            MarchingShader.Dispatch(0, threadsX, threads, threadsZ);

            Triangle[] triangles = new Triangle[(ReadTriangleCount())];
            _trianglesBuffer.GetData(triangles);

            return CreateMeshFromTriangles(triangles);
        }

        Mesh CreateMeshFromTriangles(Triangle[] triangles)
        {
            Debug.Log(triangles.Length + " " + transform.position);
            Vector3[] verts = new Vector3[triangles.Length * 3];
            int[] tris = new int[triangles.Length * 3];

            for (int i = 0; i < triangles.Length; i++)
            {
                int startIndex = i * 3;

                verts[startIndex] = triangles[i].a;
                verts[startIndex + 1] = triangles[i].b;
                verts[startIndex + 2] = triangles[i].c;

                tris[startIndex] = startIndex;
                tris[startIndex + 1] = startIndex + 1;
                tris[startIndex + 2] = startIndex + 2;
            }

            Mesh mesh = new Mesh
            {
                vertices = verts,
                triangles = tris
            };
            mesh.RecalculateNormals();

            // invert normals (they're upside down for some reason)

            for(int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] subTris = mesh.GetTriangles(i);
                for(int j = 0; j < subTris.Length; j+=3)
                {
                    // swap the values
                    (subTris[j + 1], subTris[j]) = (subTris[j], subTris[j + 1]);
                }
                mesh.SetTriangles(subTris, i);
            }
            
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = StaticResourcesLoader.Material;
            return mesh;
        }

        int ReadTriangleCount()
        {
            int[] triCount = { 0 };
            ComputeBuffer.CopyCount(_trianglesBuffer, _trianglesCountBuffer, 0);
            _trianglesCountBuffer.GetData(triCount);
            return triCount[0];
        }

        protected private void CreateBuffers(Vector3Int chunkSize)
        {
            _trianglesBuffer = new ComputeBuffer(5 * chunkSize.x * chunkSize.y * chunkSize.z, Triangle.SizeOf, ComputeBufferType.Append);
            _trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
            _weightsBuffer = new ComputeBuffer(chunkSize.x * chunkSize.y * chunkSize.z, sizeof(float));
            _points = new ComputeBuffer(chunkSize.x * chunkSize.y * chunkSize.z, sizeof(float) * 4);
        }

        protected private void ReleaseBuffers()
        {
            _trianglesBuffer.Release();
            _trianglesCountBuffer.Release();
            _weightsBuffer.Release();
            _points.Release();
        }
    }


    public enum EChunkType
    {
        Water,
        Land,
        LandGenerator
    }

    public enum ESubChunkType
    {
        //Land
        BasicIsland,
        //Water
        BasicWater
    }
}
