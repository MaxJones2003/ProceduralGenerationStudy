using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "GridCell", menuName = "PathfindingGeneration/Grid Cell")]
public class GridCell : ScriptableObject
{
    public enum RoomType { SpawnRoom, EndRoom, Room, Hallway }
    public enum CellType { Middle, Wall, Corner }

    public RoomType roomType;
    public CellType cellType;
    public GameObject cellPrefab;
    public int yRotation;
}
