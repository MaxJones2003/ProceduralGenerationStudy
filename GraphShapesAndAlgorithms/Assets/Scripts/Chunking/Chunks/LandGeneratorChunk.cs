
using Generation;
using UnityEngine;

namespace Chunking
{
    public class LandGeneratorChunk : AbstractChunk
    {
        public Map.VoronoiMapData MapData;
        public float[] _heights;
        
        public int count = 50;
        private string seed;
        private IslandShapeEnum islandShapeEnum;
        private EIslandType islandType;
        private EIslandInfluences islandInfluence;
        public override void GenerateMapData()
        {
            size = 1/* Random.Range(1, 4) */;
            count = 200/* Random.Range(50, 300) * size */;
            
            MapData = Voronoi.GenerateVoronoi(islandShapeEnum, size, count);

            _heights = Generation.HeightMap.GenerateHeights(size, MapData.corners);
            _weights = Generation.HeightMap.GenerateWeights(_heights, new Vector2(0, 0), size, false, false);
            GenerateSubChunks(size * GridMetrics.Width);
        }
        void Start()
        {
            GenerateMesh();
        }

        public override void GenerateMesh()
        {
            Vector3Int chunkSize = new Vector3Int(
                GridMetrics.PointsPerChunk/*  + 1 */,
                GridMetrics.PointsPerChunk,
                GridMetrics.PointsPerChunk/*  + 1 */
            );
            CreateBuffers(chunkSize);

            GenerateMapData();
            GetComponent<MeshFilter>().sharedMesh = ConstructMesh();

            ReleaseBuffers();
        }

        private void GenerateSubChunks(int totalSize)
        {
            int numOfChunks = totalSize / GridMetrics.PointsPerChunk;

            for (int x = 0; x < numOfChunks; x++)
            {
                for (int y = 0; y < numOfChunks; y++)
                {
                    if(x == 0 && y == 0) continue;

                    Vector3 pos = transform.position + (new Vector3(x, 0, y) * GridMetrics.PointsPerChunk);
                    LandChunk chunk = new GameObject($"Land Chunk: {pos}", 
                        new System.Type[]
                        {
                            typeof(MeshRenderer),
                            typeof(MeshFilter),
                            typeof(MeshCollider),
                            typeof(LandChunk)
                        }
                    ).GetComponent<LandChunk>();
                    chunk.transform.position = pos;
                    chunk.generatorChunk = this;
                    chunk.offset = new Vector2(x, y) * GridMetrics.PointsPerChunk;

                    chunk.endX = true/* x == numOfChunks - 1 */;
                    chunk.endZ = true/* y == numOfChunks - 1 */;
                    
                    chunk.GenerateMesh();
                }
            }
        }

    /*     int indexFromCoord2(Vector2Int coord, int _Width)
        {
            return coord.y * _Width + coord.x;
        } */
        void OnDrawGizmos()
        {
            if(_weights == null || _weights.Length == 0 || MapData == null) return;
            /* for(int i = 0; i < MapData.corners.Count; i++)
            {
                float elevation = MapData.corners[i].elevation;
                Gizmos.color = new Color(elevation, elevation, elevation);
                Vector3 pos = new Vector3(MapData.corners[i].point.x, elevation * 10, MapData.corners[i].point.y);
                Gizmos.DrawSphere(pos, 1f);
            } */
            /* int w = size * GridMetrics.Width;
            for (int u = 0; u < w; u++)
            {
                for(int v = 0; v < w; v++)
                {
                    float height = _heights[indexFromCoord2(new Vector2Int(u,v), w)];

                    Gizmos.color = new Color(height, height, height);
                    Gizmos.DrawSphere(new Vector3(u * 10, height * 10, v * 10), 1f);
                }
            } */
        /*  int width = size * GridMetrics.PointsPerChunk;
            for(int j = 0; j < _weights.Length; j++)
            {
                int x = j % width;
                int z = j / width;
                float elevation = _weights[j];
                Gizmos.color = new Color(elevation, elevation, elevation);
                Gizmos.DrawSphere(new Vector3(x, elevation * 10, z), 0.1f);
            }

            int s = size * GridMetrics.PointsPerChunk;
            for(int x = 0; x < s / 10; x++)
            {
                for(int y = 0; y < s / 10; y++)
                {
                    for(int z = 0; z < s / 10; z++)
                    {
                        int index = x + s * (y + s * z);
                        Gizmos.color = new Color(_weights[index], _weights[index], _weights[index]);
                        Gizmos.DrawSphere(new Vector3(x, y, z), 0.2f);
                    }
                }
            } */
        }
    }


    public enum EIslandType
    {
        Uninhabbited,
        Inhabited,
        Barren
    }

    public enum EIslandInfluences
    {
        Tropical,
        Hot,
        Cold,
        Frozen,
        Desert,
        Forrest
    }
}