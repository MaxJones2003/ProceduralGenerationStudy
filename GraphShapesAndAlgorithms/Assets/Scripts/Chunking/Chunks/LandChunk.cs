using UnityEngine;
namespace Chunking
{
    public class LandChunk : AbstractChunk
    {
        public LandGeneratorChunk generatorChunk;
        public Vector2 offset;
        
        public override void GenerateMapData()
        {
            _weights = Generation.HeightMap.GenerateWeights(generatorChunk._heights, offset, generatorChunk.size, endX, endZ);
        }

        public override void GenerateMesh()
        {
            Vector3Int chunkSize = new Vector3Int(
                GridMetrics.PointsPerChunk /* + (endX ? 0 : 1) */,
                GridMetrics.PointsPerChunk,/*  */
                GridMetrics.PointsPerChunk /* + (endZ ? 0 : 1) */
            );
            CreateBuffers(chunkSize);

            GenerateMapData();
            GetComponent<MeshFilter>().sharedMesh = ConstructMesh();

            ReleaseBuffers();
        }

        void OnDrawGizmos()
        {
            if(_weights == null || _weights.Length == 0) return;


            /* int width = size * GridMetrics.PointsPerChunk;
            for(int j = 0; j < _weights.Length; j++)
            {
                int x = j % width;
                int z = j / width;
                float elevation = _weights[j];
                Gizmos.color = new Color(elevation, elevation, elevation);
                Gizmos.DrawSphere(new Vector3(x, elevation * 10, z), 0.1f);
            } */

            /* int s = size * GridMetrics.PointsPerChunk;
            for(int x = 0; x < s; x++)
            {
                for(int y = 0; y < s; y++)
                {
                    for(int z = 0; z < s; z++)
                    {
                        int index = x + s * (y + s * z);
                        Gizmos.color = new Color(_weights[index], _weights[index], _weights[index]);
                        Gizmos.DrawSphere(new Vector3(x, y, z) + transform.position, 0.2f);
                    }
                }
            } */
        }
    }
}
