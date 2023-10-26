using System.Collections;
using System.Collections.Generic;
using OpenWorld;
using UnityEngine;

namespace OpenWorld
{
    public class EndlessTerrain : MonoBehaviour
    {
        public const float maxViewDst = 450;
        public Transform viewer;
        public Material mapMaterial;

        public static Vector2 viewerPosition;
        public static MapGenerator mapGenerator;
        int chunkSize;
        int chunksVisibleInViewDst;

        Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new();
        List<TerrainChunk> terrainChunksVisableLastUpdate = new();

        void Start()
        {
            mapGenerator = FindObjectOfType<MapGenerator>();
            chunkSize = MapGenerator.mapChunkSize - 1;
            chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
        }

        void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            UpdateVisibleChunks();
        }

        void UpdateVisibleChunks()
        {
            foreach(TerrainChunk chunk in terrainChunksVisableLastUpdate)
            {
                chunk.SetVisable(false);
            }
            terrainChunksVisableLastUpdate.Clear();

            int currentCunkCoordX = Mathf.RoundToInt(viewerPosition.x/chunkSize);
            int currentCunkCoordY = Mathf.RoundToInt(viewerPosition.y/chunkSize);

            for(int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
                for(int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
                {
                    Vector2 viewedChunkCoord = new Vector2(currentCunkCoordX + xOffset, currentCunkCoordY + yOffset);
                    if(terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                        if(terrainChunkDictionary[viewedChunkCoord].IsVisable())
                            terrainChunksVisableLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                    else
                    {
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
                    }
                }
        }
        public class TerrainChunk
        {
            GameObject meshObject;
            Vector2 position;
            Bounds bounds;

            MeshRenderer meshRenderer;
            MeshFilter meshFilter;
            public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
            {
                position = coord * size;
                bounds = new Bounds(position, Vector2.one*size);
                Vector3 positionv3 = new Vector3(position.x, 0, position.y);

                meshObject = new GameObject("Terrain Chunk");
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshFilter = meshObject.AddComponent<MeshFilter>();
                meshRenderer.material = material;

                meshObject.transform.position = positionv3;
                meshObject.transform.parent = parent;
                SetVisable(false);

                mapGenerator.RequestMapData(OnMapDataRecieved);
            }

            void OnMapDataRecieved(MapData mapData)
            {
                mapGenerator.RequestMeshData(mapData, OnMeshDataRecieved);
            }

            void OnMeshDataRecieved(MeshData meshData)
            {
                meshFilter.mesh = meshData.CreateMesh();
            }

            public void UpdateTerrainChunk()
            {
                float viewerDtsFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visable = viewerDtsFromNearestEdge <= maxViewDst;
                SetVisable(visable);
            }

            public void SetVisable(bool visable)
            {
                meshObject.SetActive(visable);
            }

            public bool IsVisable()
            {
                return meshObject.activeSelf;
            }
        }

        private class LODMesh
        {
            public Mesh mesh;
            public bool hasRequestedMesh;
            public bool hasMesh;
            int lod;

            public LODMesh(int lod)
            {
                this.lod = lod;
            }

            void OnMeshDataRecieved(MeshData meshData)
            {
                mesh = meshData.CreateMesh();
                hasMesh = true;
            }

            public void RequestMesh (MapData mapData)
            {
                hasRequestedMesh = true;
                mapGenerator.RequestMeshData(mapData, OnMeshDataRecieved);
            }
        }
    }

}
